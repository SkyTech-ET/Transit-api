using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transit.Api.Contracts.MOT.Request;
using Transit.Api.Contracts.MOT.Response;
using Transit.API.Helpers;
using Transit.Application;
using Transit.Application.Queries;
using Transit.Controllers;
using Transit.Domain.Data;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class AssessorController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public AssessorController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMediator mediator)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    /// <summary>
    /// Get customers pending approval
    /// </summary>
    [HttpGet("GetPendingCustomerApprovals")]
    public async Task<IActionResult> GetPendingCustomerApprovals()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var customers = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.CreatedByDataEncoder)
            .Include(c => c.Documents)
            .Where(c => !c.IsVerified && c.RecordStatus == RecordStatus.Active)
            .OrderByDescending(c => c.RegisteredDate)
            .ToListAsync();

        return HandleSuccessResponse(customers);
    }

    /// <summary>
    /// Approve or reject a customer
    /// </summary>
    [HttpPut("ApproveCustomer")]
    public async Task<IActionResult> ApproveCustomer([FromBody] CustomerApprovalRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");


        var command = new ApproveCustomerCommand
        {
            CustomerId = request.CustomerId,
            IsApproved = request.IsApproved,
            Notes = request.Notes,
            VerifiedByUserId = currentUserId.Value
        };

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Get service requests pending review
    /// </summary>
    [HttpGet("GetPendingServiceReviews")]
    public async Task<IActionResult> GetPendingServiceReviews()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = new GetPendingServiceReviewsQuery { UserId = currentUserId.Value };
        var result = await _mediator.Send(query);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }


    /// <summary>
    /// Review and approve/reject a service request
    /// </summary>
    [HttpPut("ReviewService")]
    public async Task<IActionResult> ReviewService([FromBody] ServiceReviewRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");


        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

        if (service == null)
            return NotFound("Service not found");

        if (request.IsApproved)
        {
            service.UpdateStatus(ServiceStatus.Approved);
            service.AssignAssessor(currentUserId.Value);
        }
        else
        {
            service.UpdateStatus(ServiceStatus.Rejected);
        }

        // Add review comment if provided
        if (!string.IsNullOrEmpty(request.ReviewNotes))
        {
            var reviewComment = ServiceMessage.Create(
                "Service Review",
                request.ReviewNotes,
                MessageType.System,
                request.ServiceId,
                currentUserId.Value,
                service.CreatedByDataEncoderId
            );

            _context.ServiceMessages.Add(reviewComment);
        }

        await _context.SaveChangesAsync();

        return HandleSuccessResponse(service);
    }

    /// <summary>
    /// Get services under assessor oversight
    /// </summary>
    [HttpGet("GetServicesUnderOversight")]
    public async Task<IActionResult> GetServicesUnderOversight(
        [FromQuery] ServiceStatus? status = null,
        [FromQuery] ServiceType? type = null)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.Stages)
            .Where(s => s.AssignedAssessorId == currentUserId.Value);

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
    /// Add compliance feedback to a service
    /// </summary>
    [HttpPost("AddComplianceFeedback")]
    public async Task<IActionResult> AddComplianceFeedback([FromBody] ComplianceFeedbackRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

        if (service == null)
            return NotFound("Service not found");

        var feedback = ServiceMessage.Create(
            "Compliance Feedback",
            request.Feedback,
            MessageType.System,
            request.ServiceId,
            currentUserId.Value,
            service.AssignedCaseExecutorId,
            null,
            true
        );

        _context.ServiceMessages.Add(feedback);
        await _context.SaveChangesAsync();

        return HandleSuccessResponse(feedback);
    }

    /// <summary>
    /// Get assessor dashboard
    /// </summary>
    [HttpGet("GetDashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var dashboard = new Transit.Api.Contracts.MOT.Response.AssessorDashboardResponse
        {
            PendingCustomerApprovals = await _context.Customers.CountAsync(c => !c.IsVerified && c.RecordStatus == RecordStatus.Active),
            PendingServiceReviews = await _context.Services.CountAsync(s => s.Status == ServiceStatus.Submitted),
            ServicesUnderOversight = await _context.Services.CountAsync(s => s.AssignedAssessorId == currentUserId.Value),
            CompletedReviewsToday = await _context.Services.CountAsync(s => s.AssignedAssessorId == currentUserId.Value && 
                                                                           s.LastUpdateDate.Date == DateTime.UtcNow.Date)
        };

        // Get recent activities
        dashboard.RecentCustomerApprovals = await _context.Customers
            .Include(c => c.User)
            .Where(c => c.VerifiedByUserId == currentUserId.Value)
            .OrderByDescending(c => c.VerifiedAt)
            .Take(5)
            .ToListAsync();

        dashboard.RecentServiceReviews = await _context.Services
            .Include(s => s.Customer)
            .Where(s => s.AssignedAssessorId == currentUserId.Value)
            .OrderByDescending(s => s.LastUpdateDate)
            .Take(5)
            .ToListAsync();

        return HandleSuccessResponse(dashboard);
    }

    /// <summary>
    /// Get compliance issues flagged
    /// </summary>
    [HttpGet("GetComplianceIssues")]
    public async Task<IActionResult> GetComplianceIssues()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var issues = await _context.ServiceMessages
            .Include(m => m.Service)
            .Include(m => m.SenderUser)
            .Where(m => m.MessageType == MessageType.System && 
                       m.Subject.Contains("Compliance") && 
                       m.IsUrgent)
            .OrderByDescending(m => m.RegisteredDate)
            .ToListAsync();

        return HandleSuccessResponse(issues);
    }


    private async Task<bool> IsAssessor(long userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.UserRoles.Any(ur => ur.Role.Name == "Assessor") ?? false;
    }
}
