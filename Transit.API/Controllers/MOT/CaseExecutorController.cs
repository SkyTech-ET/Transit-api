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
public class CaseExecutorController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CaseExecutorController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get assigned services for the case executor
    /// </summary>
    [HttpGet("GetAssignedServices")]
    public async Task<IActionResult> GetAssignedServices(
        [FromQuery] ServiceStatus? status = null,
        [FromQuery] ServiceType? type = null)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var query = _context.Services
            .Include(s => s.Customer)
            .Include(s => s.Stages)
            .Where(s => s.AssignedCaseExecutorId == currentUserId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (type.HasValue)
            query = query.Where(s => s.ServiceType == type.Value);

        var services = await query
            .OrderByDescending(s => s.RegisteredDate)
            .ToListAsync();

        return HandleSuccessResponse(services);
    }

    /// <summary>
    /// Get service details for execution
    /// </summary>
    [HttpGet("GetServiceById")]
    public async Task<IActionResult> GetServiceById([FromQuery] long serviceId)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var service = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.StageComments)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.Documents)
            .Include(s => s.Documents)
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.AssignedCaseExecutorId == currentUserId.Value);

        if (service == null)
            return NotFound("Service not found or not assigned to you");

        return HandleSuccessResponse(service);
    }

    /// <summary>
    /// Update service stage status
    /// </summary>
    [HttpPut("UpdateStageStatus")]
    public async Task<IActionResult> UpdateStageStatus([FromBody] UpdateStageStatusRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        // Verify service is assigned to this case executor
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.AssignedCaseExecutorId == currentUserId.Value);

        if (service == null)
            return NotFound("Service not found or not assigned to you");

        var command = new UpdateServiceStageCommand
        {
            ServiceStageId = request.StageId,
            Status = request.Status,
            Notes = request.Comments,
            UpdatedByUserId = currentUserId.Value
        };

        var result = await _mediator.Send(command);

        if (result.IsError)
            return HandleErrorResponse(result.Errors);

        // Update service status if needed
        if (request.Status == StageStatus.Completed)
        {
            // Check if all stages are completed
            var allStages = await _context.ServiceStages
                .Where(s => s.ServiceId == request.ServiceId)
                .ToListAsync();

            if (allStages.All(s => s.Status == StageStatus.Completed))
            {
                service.UpdateStatus(ServiceStatus.Completed);
                await _context.SaveChangesAsync();
            }
        }

        return HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Upload document for a service stage
    /// </summary>
    [HttpPost("UploadStageDocument")]
    public async Task<IActionResult> UploadStageDocument(
        [FromForm] long serviceId,
        [FromForm] long stageId,
        [FromForm] IFormFile file, 
        [FromForm] DocumentType documentType,
        [FromForm] string? description = null)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.AssignedCaseExecutorId == currentUserId.Value);

        if (service == null)
            return NotFound("Service not found or not assigned to you");

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
            currentUserId.Value,
            description
        );

        _context.StageDocuments.Add(document);
        await _context.SaveChangesAsync();

        return HandleSuccessResponse(document);
    }

    /// <summary>
    /// Add comment to a service stage
    /// </summary>
    [HttpPost("AddStageComment")]
    public async Task<IActionResult> AddStageComment([FromBody] AddStageCommentRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.AssignedCaseExecutorId == currentUserId.Value);

        if (service == null)
            return NotFound("Service not found or not assigned to you");

        var stage = await _context.ServiceStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId && s.ServiceId == request.ServiceId);

        if (stage == null)
            return NotFound("Service stage not found");

        var comment = StageComment.Create(
            request.Comment,
            request.StageId,
            currentUserId.Value
        );

        _context.StageComments.Add(comment);
        await _context.SaveChangesAsync();

        return HandleSuccessResponse(comment);
    }

    /// <summary>
    /// Set risk level for a service
    /// </summary>
    [HttpPut("SetRiskLevel")]
    public async Task<IActionResult> SetRiskLevel([FromBody] SetRiskLevelRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.AssignedCaseExecutorId == currentUserId.Value);

        if (service == null)
            return NotFound("Service not found or not assigned to you");

        service.UpdateRiskLevel(request.RiskLevel);

        // Add risk notes to the current stage if provided
        if (!string.IsNullOrEmpty(request.RiskNotes))
        {
            var currentStage = await _context.ServiceStages
                .Where(s => s.ServiceId == request.ServiceId)
                .OrderByDescending(s => s.RegisteredDate)
                .FirstOrDefaultAsync();

            if (currentStage != null)
            {
                currentStage.AddRiskNotes(request.RiskNotes);
            }
        }

        await _context.SaveChangesAsync();

        return HandleSuccessResponse(service);
    }

    /// <summary>
    /// Block a service stage
    /// </summary>
    [HttpPut("BlockStage")]
    public async Task<IActionResult> BlockStage([FromBody] BlockStageRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.AssignedCaseExecutorId == currentUserId.Value);

        if (service == null)
            return NotFound("Service not found or not assigned to you");

        var stage = await _context.ServiceStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId && s.ServiceId == request.ServiceId);

        if (stage == null)
            return NotFound("Service stage not found");

        stage.SetBlocked(true, request.Reason);

        await _context.SaveChangesAsync();

        return HandleSuccessResponse(stage);
    }

    /// <summary>
    /// Get case executor dashboard
    /// </summary>
    [HttpGet("GetDashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsCaseExecutor(currentUserId.Value))
            return Forbid("Access denied. Case Executor role required.");

        var dashboard = new Transit.Api.Contracts.MOT.Response.CaseExecutorDashboardResponse
        {
            AssignedServices = await _context.Services.CountAsync(s => s.AssignedCaseExecutorId == currentUserId.Value),
            PendingServices = await _context.Services.CountAsync(s => s.AssignedCaseExecutorId == currentUserId.Value && s.Status == ServiceStatus.InProgress),
            CompletedServices = await _context.Services.CountAsync(s => s.AssignedCaseExecutorId == currentUserId.Value && s.Status == ServiceStatus.Completed),
            BlockedStages = await _context.ServiceStages.CountAsync(s => s.Service.AssignedCaseExecutorId == currentUserId.Value && s.IsBlocked)
        };

        // Get today's tasks
        dashboard.TodaysTasks = await _context.ServiceStages
            .Include(s => s.Service)
            .Where(s => s.Service.AssignedCaseExecutorId == currentUserId.Value && 
                       s.Status == StageStatus.Pending &&
                       s.RegisteredDate.Date == DateTime.UtcNow.Date)
            .ToListAsync();

        // Get urgent notifications
        dashboard.UrgentNotifications = await _context.Notifications
            .Include(n => n.Service)
            .Where(n => n.UserId == currentUserId.Value && n.IsUrgent && !n.IsRead)
            .ToListAsync();

        return HandleSuccessResponse(dashboard);
    }


    private async Task<bool> IsCaseExecutor(long userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.UserRoles.Any(ur => ur.Role.Name == "CaseExecutor") ?? false;
    }
}
