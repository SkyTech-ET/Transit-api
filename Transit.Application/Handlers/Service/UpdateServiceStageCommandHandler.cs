using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using System.Text.Json;

namespace Transit.Application;

internal class UpdateServiceStageCommandHandler : IRequestHandler<UpdateServiceStageCommand, OperationResult<ServiceStageExecution>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession? _session => _httpContextAccessor.HttpContext?.Session;
    private readonly TokenHandlerService _tokenHandlerService;

    public UpdateServiceStageCommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }

    public async Task<OperationResult<ServiceStageExecution>> Handle(UpdateServiceStageCommand request, CancellationToken cancellationToken)
    {


        var result = new OperationResult<ServiceStageExecution>();
        // Verify service stage exists
        var serviceStage = await _context.ServiceStages
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceStageId, cancellationToken);

        if (serviceStage == null)
        {
            result.AddError(ErrorCode.NotFound, "Service stage not found.");
            return result;
        }

        // Update stage status
        serviceStage.UpdateStatus(request.Status, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        result.Payload = serviceStage;
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




