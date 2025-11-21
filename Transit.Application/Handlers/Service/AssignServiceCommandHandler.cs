using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using System.Text.Json;

namespace Transit.Application;

internal class AssignServiceCommandHandler : IRequestHandler<AssignServiceCommand, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession? _session => _httpContextAccessor.HttpContext?.Session;
    private readonly TokenHandlerService _tokenHandlerService;

    public AssignServiceCommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }

    public async Task<OperationResult<Service>> Handle(AssignServiceCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        long userId = 0;
        if (userName.Length > 0)
        {
            var existingUsers = await _context.Users
                      .FirstOrDefaultAsync(x => x.Username == userName);
            if (existingUsers != null)
                userId = existingUsers.Id;
        }

        // Verify service exists
        var service = await _context.Services
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found.");
            return result;
        }

        // Verify Case Executor exists
        var caseExecutor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AssignedCaseExecutorId, cancellationToken);

        if (caseExecutor == null)
        {
            result.AddError(ErrorCode.NotFound, "Case Executor not found.");
            return result;
        }

        // Assign service
        service.AssignCaseExecutor(request.AssignedCaseExecutorId);
        if (request.AssignedAssessorId.HasValue)
        {
            service.AssignAssessor(request.AssignedAssessorId.Value);
        }
        service.UpdateStatus(ServiceStatus.InProgress);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload service with relationships
        var updatedService = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (updatedService == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found after assignment.");
            return result;
        }

        result.Payload = updatedService;
        result.Message = "Operation success";

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };

        return result;
    }

    private string? GetCurrentUserName()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return null;

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var claims = _tokenHandlerService.GetClaims(token);
        var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
        return userNameClaim?.Value;
    }
}

