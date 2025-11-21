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
public class ManagerController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public ManagerController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMediator mediator)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard analytics and key metrics
    /// </summary>
    [HttpGet("GetDashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Verify user is manager
        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var dashboard = new Transit.Api.Contracts.MOT.Response.ManagerDashboardResponse
        {
            TotalServices = await _context.Services.CountAsync(),
            PendingServices = await _context.Services.CountAsync(s => s.Status == ServiceStatus.Submitted),
            InProgressServices = await _context.Services.CountAsync(s => s.Status == ServiceStatus.InProgress),
            CompletedServices = await _context.Services.CountAsync(s => s.Status == ServiceStatus.Completed),
            TotalCustomers = await _context.Customers.CountAsync(),
            VerifiedCustomers = await _context.Customers.CountAsync(c => c.IsVerified),
            TotalStaff = await _context.Users.CountAsync(),
            ActiveStaff = await _context.Users.CountAsync(u => u.RecordStatus == RecordStatus.Active)
        };

        // Get recent services
        dashboard.RecentServices = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .OrderByDescending(s => s.RegisteredDate)
            .Take(10)
            .ToListAsync();

        // Get service completion rates by month
        var currentDate = DateTime.UtcNow;
        var sixMonthsAgo = currentDate.AddMonths(-6);
        
        dashboard.MonthlyServiceStats = await _context.Services
            .Where(s => s.RegisteredDate >= sixMonthsAgo)
            .GroupBy(s => new { s.RegisteredDate.Year, s.RegisteredDate.Month })
            .Select(g => new MonthlyServiceStat
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalServices = g.Count(),
                CompletedServices = g.Count(s => s.Status == ServiceStatus.Completed),
                CompletionRate = g.Count(s => s.Status == ServiceStatus.Completed) * 100.0 / g.Count()
            })
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ToListAsync();

        return HandleSuccessResponse(dashboard);
    }

    /// <summary>
    /// Get all services with filtering options
    /// </summary>
    [HttpGet("GetAllServices")]
    public async Task<IActionResult> GetAllServices(
        [FromQuery] ServiceStatus? status = null,
        [FromQuery] ServiceType? type = null,
        [FromQuery] long? customerId = null,
        [FromQuery] long? caseExecutorId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var query = _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.CreatedByDataEncoder)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (type.HasValue)
            query = query.Where(s => s.ServiceType == type.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (caseExecutorId.HasValue)
            query = query.Where(s => s.AssignedCaseExecutorId == caseExecutorId.Value);

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
    /// Get service details for management oversight
    /// </summary>
    [HttpGet("GetServiceById")]
    public async Task<IActionResult> GetServiceById([FromQuery] long serviceId)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var service = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.CreatedByDataEncoder)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.StageComments)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.Documents)
            .Include(s => s.Documents)
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
            return NotFound("Service not found");

        return HandleSuccessResponse(service);
    }

    /// <summary>
    /// Assign case executor to a service
    /// </summary>
    [HttpPut("AssignExecutor")]
    public async Task<IActionResult> AssignExecutor([FromBody] AssignExecutorRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var command = new AssignServiceCommand
        {
            ServiceId = request.ServiceId,
            AssignedCaseExecutorId = request.CaseExecutorId,
            AssignedAssessorId = null,
            AssignmentNotes = null,
            AssignedByUserId = currentUserId.Value
        };

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Get all customers with filtering
    /// </summary>
    [HttpGet("GetAllCustomers")]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] bool? isVerified = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var query = _context.Customers
            .Include(c => c.User)
            .Include(c => c.CreatedByDataEncoder)
            .Include(c => c.VerifiedByUser)
            .AsQueryable();

        if (isVerified.HasValue)
            query = query.Where(c => c.IsVerified == isVerified.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => 
                c.BusinessName.Contains(searchTerm) ||
                c.TINNumber.Contains(searchTerm) ||
                c.ContactEmail.Contains(searchTerm));
        }

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
    /// Get all staff members
    /// </summary>
    [HttpGet("GetAllStaff")]
    public async Task<IActionResult> GetAllStaff([FromQuery] string? role = null)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.RecordStatus == RecordStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
        }

        var staff = await query.ToListAsync();

        return HandleSuccessResponse(staff);
    }

    /// <summary>
    /// Get system notifications for managers
    /// </summary>
    [HttpGet("GetNotifications")]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        if (!await IsManager(currentUserId.Value))
            return Forbid("Access denied. Manager role required.");

        var query = _context.Notifications
            .Include(n => n.Service)
            .Include(n => n.User)
            .Where(n => n.Type == NotificationType.SystemAlert || n.IsUrgent);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.RegisteredDate)
            .ToListAsync();

        return HandleSuccessResponse(notifications);
    }


    private async Task<bool> IsManager(long userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.UserRoles.Any(ur => ur.Role.Name == "Manager") ?? false;
    }
}
