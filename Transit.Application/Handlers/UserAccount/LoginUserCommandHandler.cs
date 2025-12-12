using Transit.Domain.Models.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Transit.Application;

internal class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, OperationResult<UserLoginDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly TokenHandlerService _tokenService;
    // private readonly ActionLogService _actionLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public LoginUserCommandHandler(ApplicationDbContext context, PasswordService passwordService,
        TokenHandlerService tokenService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordService = passwordService;
        //_actionLogService = actionLogService;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResult<UserLoginDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<UserLoginDto>();
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        long userId = 0;
        var user = await _context.Users.Where(x => x.Username == request.UserName && x.AccountStatus == AccountStatus.Approved)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePrivileges)
                        .ThenInclude(x => x.Privilege)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            result.AddError(ErrorCode.UserDoesNotExist, "User doesn't exist.");
            return result;
        }
        userId = user.Id;

        if (!_passwordService.ValidatePassword(user.Password, request.Password))
        {
            result.AddError(ErrorCode.IncorrectPassword, "Invalid password.");
            return result;
        }
        if (user.IsAccountLocked)
        {
            result.AddError(ErrorCode.ServerError, "Your account is locked");
            return result;
        }
        if (user.RecordStatus != Transit.Domain.Models.Shared.RecordStatus.Active)
        {
            result.AddError(ErrorCode.UserDoesNotExist, "User doesn't exist.");
            return result;
        }

        var User = new UserLoginDto(user.Username, user.Email, user.FirstName, user.LastName, user.Phone, user.ProfilePhoto);
        User.Id = user.Id;
        User.Roles = _getRole(user);
        User.AccessToken = GetJwtString(user.userTokenLifetime, _getClaim(User));
        User.RefreshToken = SetRefreshToken(user);

        result.Payload = User;

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };

        //await _actionLogService.LogActionAsync(
        //    userId,
        //    JsonSerializer.Serialize(request, options),
        //    JsonSerializer.Serialize(result, options),
        //    "LoginUser"
        //);
        return result;
    }

    private List<RoleDto> _getRole(User user)
    {
        var result = new List<RoleDto>();
        if (user.UserRoles.Any())
        {
            foreach (var userRole in user.UserRoles)
            {
                var roleDto = new RoleDto();
                roleDto.Id = userRole.Role.Id;
                roleDto.RoleName = userRole.Role.Name;
                foreach (var roleClaim in userRole.Role.RolePrivileges)
                {
                    roleDto.Privileges.Add(new Dtos.PrivilegeDto
                    {
                        Id = roleClaim.PrivilegeId,
                        Action = roleClaim.Privilege.Action
                    });
                }
                result.Add(roleDto);
            }
        }
        return result;
    }
    private List<Claim> _getClaim(UserLoginDto user)
    {
        var result = new List<Claim>();
        result.Add(new Claim("userName", user.Username));
        result.Add(new Claim("id", user.Id.ToString()));
        foreach (var role in user.Roles)
        {
            foreach (var userPrivilege in role.Privileges)
            {
                Claim claim = new Claim(userPrivilege.Id.ToString(), String.Format("{0}", userPrivilege.Action));
                result.Add(claim);
            }
        }
        return result;
    }
    private string GetJwtString(int tokenLife, List<Claim> claimList)
    {
        var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
         }
            .Union(claimList).ToList();
        var token = _tokenService.CreateSecurityToken(claims, tokenLife);
        return _tokenService.WriteToken(token);
    }
    public string SetRefreshToken(User user)
    {
        user.RefreshTokenExpireDate = DateTime.UtcNow.AddMinutes(user.userTokenLifetime);
        user.RefreshToken = _tokenService.GenerateRefreshToken();
        _context.Users.Update(user);
        _context.SaveChanges();
        return user.RefreshToken;
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
