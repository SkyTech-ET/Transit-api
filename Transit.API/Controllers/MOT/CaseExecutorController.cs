using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transit.Api.Contracts.MOT.Request;
using Transit.Api.Contracts.MOT.Response;
using Transit.Api.Contracts.User.Response;
using Transit.API.DTO.MOT;
using Transit.API.Helpers;
using Transit.Application;
using Transit.Application.Queries;
using Transit.Controllers;
using Transit.Domain.Data;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using Transit.Application.Commands;


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
    public async Task<IActionResult> GetAssignedServices([FromQuery] long AssignedCaseExecutorId, RecordStatus? recordStatus)
    {
        var query = new GetAssignedServicesQuery { AssignedCaseExecutorId = AssignedCaseExecutorId, RecordStatus = recordStatus };
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<ServiceDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    /// <summary>
    /// Get service details for execution
    /// </summary>
    [HttpGet("GetAssignedServiceById")]
    public async Task<IActionResult> GetAssignedServiceById([FromQuery] long Id)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = new GetCaseExecutorAssignedServicesByIdQuery { AssignedCaseExecutorId = currentUserId.Value, Id = Id };
        var result = await _mediator.Send(query);
       // var rolesList = result.Payload.Adapt<List<ServiceDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }



    /// <summary>
    /// Upload document for a service stage
    /// </summary>
    [HttpPost("UploadStageDocument")]
    public async Task<IActionResult> UploadStageDocument([FromForm] UploadDocumentRequest request)
    {
        // 1?? Validate file
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new
            {
                Error = true,
                Message = "Document file is required."
            });
        }

        var command = new UploadDocumentCommand
        {
            ServiceId = request.ServiceId,
            StageId = request.StageId,
            File = request.File,
            DocumentType = request.DocumentType,
            Description = request.Description
            // FilePath can be added if already saved
        };


        // 6?? Send command to handler
        var result = await _mediator.Send(command);

        // 7?? Map payload to response DTO if needed
        var documentDetail = result.Payload?.Adapt<DocumentDetail>();

        // 8?? Return response
        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(documentDetail);
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

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

        if (service == null)
            return NotFound("Service not found");

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
