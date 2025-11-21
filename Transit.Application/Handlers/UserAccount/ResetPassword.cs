using System.Text.Json;

namespace Transit.Application;
public record ResetPasswordCommand(string UserName, string Password) : IRequest<OperationResult<Unit>>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly TokenHandlerService _tokenService;
    // private readonly ActionLogService _actionLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ResetPasswordCommandHandler(ApplicationDbContext context, PasswordService passwordService,
        TokenHandlerService tokenService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _tokenService = tokenService;
        //_actionLogService = actionLogService;
        _httpContextAccessor = httpContextAccessor;
        _passwordService = passwordService;
    }
    public async Task<OperationResult<Unit>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Unit>();
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        long userId = 0;
        if (userName.Length > 0)
        {
            var existingUser = await _context.Users
                      .FirstOrDefaultAsync(x => x.Username == userName);
            userId = existingUser.Id;
        }
        var userAccount = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.UserName);
        if (userAccount is null)
        {
            result.AddError(ErrorCode.NotFound, "Account Does not Exist");
            return result;
        }
        userAccount.UpdatePassword(_passwordService.HashPassword(request.Password));
        _context.Users.Update(userAccount);
        _context.SaveChanges();
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
        //    "ResetPassword"
        // );
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
        var claims = _tokenService.GetClaims(token); // Use TokenHandlerService to get claims

        var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
        return userNameClaim?.Value; // Return the username or null if not found
    }
}


