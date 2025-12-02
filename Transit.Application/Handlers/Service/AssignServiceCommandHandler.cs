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

        // Verify service exists
        var service = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.Stages)
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

        // Assign Case Executor
        service.AssignCaseExecutor(request.AssignedCaseExecutorId);

        // Assign Assessor if provided
        if (request.AssignedAssessorId.HasValue)
        {
            var assessor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.AssignedAssessorId.Value, cancellationToken);

            if (assessor == null)
            {
                result.AddError(ErrorCode.NotFound, "Assessor not found.");
                return result;
            }

            service.AssignAssessor(request.AssignedAssessorId.Value);
        }

        // Optionally store assignment notes
        if (!string.IsNullOrWhiteSpace(request.AssignmentNotes))
        {
            service.AssignmentNotes = request.AssignmentNotes.Trim();
        }

        // Update service status
        service.UpdateStatus(ServiceStatus.InProgress);

        await _context.SaveChangesAsync(cancellationToken);

        result.Payload = service;
        result.Message = "Service assignment successful.";

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

