using Transit.Domain.Models.Shared;
using System.Text.Json;

namespace Transit.Application;
public record DeleteUser(long id) : IRequest<OperationResult<Unit>>;

internal class DeleteUserHandler : IRequestHandler<DeleteUser, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    // private readonly ActionLogService _actionLogService;
    private readonly TokenHandlerService _tokenHandlerService;

    public DeleteUserHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        //  ActionLogService actionLogService,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        // _actionLogService = actionLogService;
        _tokenHandlerService = tokenHandlerService;
    }
    public async Task<OperationResult<Unit>> Handle(DeleteUser request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Unit>();


        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.id && x.RecordStatus != RecordStatus.Deleted);
        if (existingUser is null)
        {
            result.AddError(ErrorCode.NotFound, "User Not found.");
            return result;
        }
        existingUser.UpdateStatus(Domain.Models.Shared.RecordStatus.Deleted);
        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();
        result.Message = "Operation success";
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };

        //await _actionLogService.LogActionAsync(
        //    userId,
        //    JsonSerializer.Serialize(request, options),
        //    JsonSerializer.Serialize(result, options),
        //    "DeleteUser"
        //);
        return result;
    }
    // Helper method to get the currently logged-in UserName
    private string GetCurrentUserName()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null; // No token available in request
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var claims = _tokenHandlerService.GetClaims(token); // Use TokenHandlerService to get claims

        var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
        return userNameClaim?.Value; // Return the username or null if not found
    }
}
