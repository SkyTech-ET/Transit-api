using Microsoft.AspNetCore.Mvc;
using Transit.Domain.Data;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using Microsoft.EntityFrameworkCore;
using Transit.Controllers;
using Transit.API.Helpers;
using Transit.Application;
using Transit.Api.Contracts.MOT.Request;
using Transit.Api.Contracts.MOT.Response;
using Mapster;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomerController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomerController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Create a new service request as a customer
    /// </summary>
    [HttpPost("CreateService")]
    public async Task<IActionResult> CreateService([FromBody] CreateCustomerServiceRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Verify customer exists and is verified
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == currentUserId.Value && c.IsVerified);

        if (customer == null)
            return BadRequest("Customer not found or not verified");

        var command = request.Adapt<CreateServiceRequestCommand>();
        command.CustomerId = customer.Id;
        command.CreatedByUserId = currentUserId.Value;

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Get all services for the current customer
    /// </summary>
    [HttpGet("GetMyServices")]
    public async Task<IActionResult> GetMyServices([FromQuery] ServiceStatus? status = null)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Get customer for current user
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == currentUserId.Value);

        if (customer == null)
            return BadRequest("Customer profile not found");

        var query = _context.Services
            .Include(s => s.Stages)
            .Include(s => s.Documents)
            .Where(s => s.CustomerId == customer.Id); // Use customer.Id, not customer.UserId

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var services = await query.ToListAsync();

        return HandleSuccessResponse(services);
    }

    /// <summary>
    /// Get service details with full workflow timeline
    /// </summary>
    [HttpGet("GetServiceById")]
    public async Task<IActionResult> GetServiceById([FromQuery] long serviceId)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Get customer for current user
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == currentUserId.Value);

        if (customer == null)
            return BadRequest("Customer profile not found");

        var service = await _context.Services
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.StageComments)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.Documents)
            .Include(s => s.Documents)
            .Include(s => s.Messages)
            .Include(s => s.AssignedCaseExecutor)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.CustomerId == customer.Id); // Use customer.Id

        if (service == null)
            return NotFound("Service not found");

        return HandleSuccessResponse(service);
    }

    /// <summary>
    /// Upload document for a service stage
    /// </summary>
    [HttpPost("UploadStageDocument")]
    public async Task<IActionResult> UploadStageDocument(
        [FromForm] long serviceId,
        [FromForm] long stageId,
        [FromForm] IFormFile file,
        [FromForm] DocumentType documentType)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Verify service belongs to customer
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.CustomerId == currentUserId.Value);
        
        if (service == null)
            return NotFound("Service not found");

        var stage = await _context.ServiceStages
            .FirstOrDefaultAsync(s => s.Id == stageId && s.ServiceId == serviceId);
        
        if (stage == null)
            return NotFound("Service stage not found");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Save file
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "service-documents");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Create document record
        var document = StageDocument.Create(
            uniqueFileName,
            Path.Combine("service-documents", uniqueFileName),
            file.FileName,
            Path.GetExtension(file.FileName),
            file.Length,
            file.ContentType,
            documentType,
            stageId,
            currentUserId.Value
        );

        _context.StageDocuments.Add(document);
        await _context.SaveChangesAsync();

        return HandleSuccessResponse(document);
    }

    /// <summary>
    /// Get customer notifications
    /// </summary>
    [HttpGet("GetNotifications")]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = _context.Notifications
            .Include(n => n.Service)
            .Where(n => n.UserId == currentUserId.Value);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.RegisteredDate)
            .ToListAsync();

        return HandleSuccessResponse(notifications);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("MarkNotificationAsRead")]
    public async Task<IActionResult> MarkNotificationAsRead([FromQuery] long notificationId)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId.Value);

        if (notification == null)
            return NotFound("Notification not found");

        notification.MarkAsRead();
        await _context.SaveChangesAsync();

        return HandleSuccessResponse(notification);
    }

    /// <summary>
    /// Get customer profile information
    /// </summary>
    [HttpGet("GetProfile")]
    public async Task<IActionResult> GetProfile()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var customer = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.UserId == currentUserId.Value);

        if (customer == null)
            return NotFound("Customer profile not found");

        return HandleSuccessResponse(customer);
    }

    /// <summary>
    /// Update customer profile
    /// </summary>
    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == currentUserId.Value);

        if (customer == null)
            return NotFound("Customer profile not found");

        customer.UpdateBusinessInfo(
            request.BusinessName,
            request.BusinessAddress,
            request.City,
            request.State,
            request.PostalCode,
            request.ContactPerson,
            request.ContactPhone,
            request.ContactEmail
        );

        await _context.SaveChangesAsync();

        return HandleSuccessResponse(customer);
    }

    private async Task CreateServiceStages(long serviceId, ServiceType serviceType)
    {
        var stages = new List<ServiceStageExecution>();

        // Create stages based on service type
        switch (serviceType)
        {
            case ServiceType.Multimodal:
                stages.AddRange(new[]
                {
                    ServiceStageExecution.Create(serviceId, ServiceStage.PrepaymentInvoice),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DropRisk),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DeliveryOrder),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Inspection),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Transportation),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Clearance)
                });
                break;
            case ServiceType.Unimodal:
                stages.AddRange(new[]
                {
                    ServiceStageExecution.Create(serviceId, ServiceStage.PrepaymentInvoice),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DropRisk),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DeliveryOrder),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Inspection),
                    ServiceStageExecution.Create(serviceId, ServiceStage.LocalPermission),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Arrival),
                    ServiceStageExecution.Create(serviceId, ServiceStage.StoreSettlement)
                });
                break;
            default:
                stages.AddRange(new[]
                {
                    ServiceStageExecution.Create(serviceId, ServiceStage.PrepaymentInvoice),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DropRisk),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DeliveryOrder)
                });
                break;
        }

        _context.ServiceStages.AddRange(stages);
        await _context.SaveChangesAsync();
    }
}
