using Microsoft.AspNetCore.Mvc;
using Transit.Domain.Data;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using Transit.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Transit.Controllers;
using Transit.API.Helpers;
using Transit.API.DTO.MasterData.Request;
using Transit.Api.Contracts.MOT.Request;
using Transit.Api.Contracts.MOT.Response;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class DataEncoderController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DataEncoderController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost("CreateCustomer")]
    public async Task<IActionResult> CreateCustomer([FromBody] Transit.API.DTO.MasterData.Request.CreateCustomerRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsDataEncoder(currentUserId.Value))
            return Forbid("Access denied. Data Encoder role required.");

        // Check if user already exists
        var existingUser = await _context.Customers
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (existingUser != null)
            return BadRequest("User with this email or username already exists");

        // Create customer profile
        var customer = Customer.Create(
            request.BusinessName,
            request.TINNumber,
            request.BusinessLicense,
            request.BusinessAddress,
            request.City,
            request.State,
            request.PostalCode,
            request.ContactPerson,
            request.ContactPhone,
            request.ContactEmail,
            request.BusinessType,
            request.ImportLicense,
            request.ImportLicenseExpiry,
            request.UserId,
            currentUserId.Value
        );

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return HandleSuccessResponse(customer);
    }

    /// <summary>
    /// Get all customers created by the data encoder
    /// </summary>
    [HttpGet("GetAllCustomers")]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] bool? isVerified = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsDataEncoder(currentUserId.Value))
            return Forbid("Access denied. Data Encoder role required.");

        var query = _context.Customers
            .Include(c => c.User)
            .Include(c => c.VerifiedByUser)
            .Where(c => c.CreatedByDataEncoderId == currentUserId.Value);

        if (isVerified.HasValue)
            query = query.Where(c => c.IsVerified == isVerified.Value);

        var totalCount = await query.CountAsync();
        var customers = await query
            .OrderByDescending(c => c.RegisteredDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new Transit.Api.Contracts.MOT.Response.PaginatedResult<Customer>
        {
            Data = customers,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return HandleSuccessResponse(result);
    }

    /// <summary>
    /// Create a new service request
    /// </summary>
    [HttpPost("CreateService")]
    public async Task<IActionResult> CreateService([FromBody] Transit.Api.Contracts.MOT.Request.CreateServiceRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsDataEncoder(currentUserId.Value))
            return Forbid("Access denied. Data Encoder role required.");

        // Verify customer exists and is verified
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.IsVerified);

        if (customer == null)
            return BadRequest("Customer not found or not verified");

        // Generate service number
        var serviceNumber = $"SRV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var service = Service.Create(
            serviceNumber,
            request.ItemDescription,
            request.RouteCategory,
            request.DeclaredValue,
            request.TaxCategory,
            request.CountryOfOrigin,
            request.ServiceType,
            request.CustomerId,
            currentUserId.Value
        );

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        // Create initial service stages based on service type
        await CreateServiceStages(service.Id, request.ServiceType);

        return HandleSuccessResponse(service);
    }

    /// <summary>
    /// Get service requests created by the data encoder
    /// </summary>
    [HttpGet("GetAllServices")]
    public async Task<IActionResult> GetAllServices(
        [FromQuery] ServiceStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsDataEncoder(currentUserId.Value))
            return Forbid("Access denied. Data Encoder role required.");

        var query = _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Where(s => s.CreatedByDataEncoderId == currentUserId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var totalCount = await query.CountAsync();
        var services = await query
            .OrderByDescending(s => s.RegisteredDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new Transit.Api.Contracts.MOT.Response.PaginatedResult<Service>
        {
            Data = services,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return HandleSuccessResponse(result);
    }

    /// <summary>
    /// Update customer information before approval
    /// </summary>
    [HttpPut("UpdateCustomer")]
    public async Task<IActionResult> UpdateCustomer([FromBody] Transit.Api.Contracts.MOT.Request.UpdateCustomerRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsDataEncoder(currentUserId.Value))
            return Forbid("Access denied. Data Encoder role required.");

        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.CreatedByDataEncoderId == currentUserId.Value);

        if (customer == null)
            return NotFound("Customer not found or not created by you");

        if (customer.IsVerified)
            return BadRequest("Cannot update verified customer");

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

    /// <summary>
    /// Get data encoder dashboard
    /// </summary>
    [HttpGet("GetDashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsDataEncoder(currentUserId.Value))
            return Forbid("Access denied. Data Encoder role required.");

        var dashboard = new Transit.Api.Contracts.MOT.Response.DataEncoderDashboardResponse
        {
            TotalCustomersCreated = await _context.Customers.CountAsync(c => c.CreatedByDataEncoderId == currentUserId.Value),
            PendingCustomerApprovals = await _context.Customers.CountAsync(c => c.CreatedByDataEncoderId == currentUserId.Value && !c.IsVerified),
            TotalServicesCreated = await _context.Services.CountAsync(s => s.CreatedByDataEncoderId == currentUserId.Value),
            PendingServiceApprovals = await _context.Services.CountAsync(s => s.CreatedByDataEncoderId == currentUserId.Value && s.Status == ServiceStatus.Submitted),
            DraftServices = await _context.Services.CountAsync(s => s.CreatedByDataEncoderId == currentUserId.Value && s.Status == ServiceStatus.Draft)
        };

        // Get recent activities
        dashboard.RecentCustomers = await _context.Customers
            .Include(c => c.User)
            .Where(c => c.CreatedByDataEncoderId == currentUserId.Value)
            .OrderByDescending(c => c.RegisteredDate)
            .Take(5)
            .ToListAsync();

        dashboard.RecentServices = await _context.Services
            .Include(s => s.Customer)
            .Where(s => s.CreatedByDataEncoderId == currentUserId.Value)
            .OrderByDescending(s => s.RegisteredDate)
            .Take(5)
            .ToListAsync();

        return HandleSuccessResponse(dashboard);
    }

    private async Task CreateServiceStages(long serviceId, ServiceType serviceType)
    {
        var stages = new List<ServiceStage>
        {
            ServiceStage.PrepaymentInvoice,
            ServiceStage.DropRisk,
            ServiceStage.DeliveryOrder,
            ServiceStage.Inspection,
            ServiceStage.Emergency,
            ServiceStage.Exit,
            ServiceStage.Transportation,
            ServiceStage.Clearance,
            ServiceStage.StoreSettlement
        };

        // Add unimodal-specific stages
        if (serviceType == ServiceType.Unimodal)
        {
            stages.Add(ServiceStage.LocalPermission);
            stages.Add(ServiceStage.Arrival);
        }

        foreach (var stage in stages)
        {
            var serviceStage = ServiceStageExecution.Create(serviceId, stage);
            _context.ServiceStages.Add(serviceStage);
        }

        await _context.SaveChangesAsync();
    }

    private long? GetCurrentUserId()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return null;

        return 1; // This should be extracted from the JWT token
    }

    private async Task<bool> IsDataEncoder(long userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.UserRoles.Any(ur => ur.Role.Name == "DataEncoder") ?? false;
    }
}



