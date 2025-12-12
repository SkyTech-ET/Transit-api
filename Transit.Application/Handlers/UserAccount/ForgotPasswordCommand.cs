using Mapster;
using System.Text.Json;
namespace Transit.Application;

public record ForgotPasswordCommand(string UserName) : IRequest<OperationResult<Unit>>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSenderService _emailSenderService;
    private readonly TokenHandlerService _tokenHandlerService;
    //private readonly ActionLogService _actionLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ForgotPasswordCommandHandler(ApplicationDbContext context, TokenHandlerService tokenHandlerService,
        EmailSenderService emailSenderService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _emailSenderService = emailSenderService;
        _tokenHandlerService = tokenHandlerService;
        // _actionLogService = actionLogService;
        _httpContextAccessor = httpContextAccessor;

    }
    public async Task<OperationResult<Unit>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
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
        var userAccount = await _context.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Username == request.UserName);
        if (userAccount is null)
        {
            result.AddError(ErrorCode.NotFound, "Account Does not Exist");
            return result;
        }

        var Roles = await GetRole(userAccount);
        string token = _tokenHandlerService.GetJwtString(30/* five minutes to expire*/,
            _tokenHandlerService.GetClaimFromRole(Roles, userAccount.Username, userAccount.Id));
        if (String.IsNullOrEmpty(token))
        {
            result.AddError(ErrorCode.IncorrectPassword, "Invalide Token.");
            return result;
        }

        string CallbakUrl = "https://wwww.Client.web/#/reset-password?activation_token=" + token;
        string message = "<p>Please reset your password by clicking <a href='" + CallbakUrl + "'>here</a></p>";

        await _emailSenderService.SendEmailAsync(message, "Forgot Password", new string[] { userAccount.Email }, null, null);
        result.Message = "If the User Name is known to us we send the password reset link";
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };

        //await _actionLogService.LogActionAsync(
        //    userId,
        //    JsonSerializer.Serialize(request, options),
        //    JsonSerializer.Serialize(result, options),
        //    "ForgotPassword"
        //);
        return result;
    }

    private async Task<List<RoleDto>> GetRole(User user)
    {
        var result = new List<RoleDto>();
        if (user.UserRoles.Any())
        {
            var roles = await _context.Role.
                Where(x => user.UserRoles.Select(x => x.RoleId).ToList().Contains(x.Id))
                .Include(x => x.RolePrivileges)
                .ToListAsync();
            foreach (var role in roles)
            {
                var roleDto = new RoleDto();
                var claimIds = role.RolePrivileges.Select(x => x.PrivilegeId).ToList();
                var claims = _context.Privilege.Where(x => claimIds.Contains(x.Id)).ToList();
                roleDto.Privileges = claims.Adapt<List<PrivilegeDto>>();
                result.Add(roleDto);
            }
        }
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

