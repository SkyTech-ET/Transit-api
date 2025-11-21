using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Transit.Application;

public record RefreshToken(string refreshToken) : IRequest<OperationResult<UserLoginDto>>;
internal class RefreshTokenHandler : IRequestHandler<RefreshToken, OperationResult<UserLoginDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly TokenHandlerService _tokenService;
    public RefreshTokenHandler(ApplicationDbContext context, TokenHandlerService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    public async Task<OperationResult<UserLoginDto>> Handle(RefreshToken request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<UserLoginDto>();
        try
        {
            var user = await _context.Users.Where(x => x.RefreshToken == request.refreshToken)
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePrivileges)
                        .ThenInclude(x => x.Privilege)
            .FirstOrDefaultAsync();
            if (user is null)
            {
                result.AddError(ErrorCode.NotFound, "User doesn't exist .");
                return result;
            }
            if (user.IsAccountLocked)
            {
                result.AddError(ErrorCode.ServerError, "Your account is locked");
                return result;
            }
            if (user.RecordStatus != Domain.Models.Shared.RecordStatus.Active)
            {
                result.AddError(ErrorCode.NotFound, "User doesn't exist.");
                return result;
            }
            var identityUser = new UserLoginDto() { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, Username = user.Username };
            identityUser.Id = user.Id;
            identityUser.AccessToken = GetJwtString(user.userTokenLifetime, _getClaim(identityUser));
            identityUser.RefreshToken = _setRefreshToken(user);
            result.Payload = identityUser;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
            return result;
        }
    }
    private static async Task<List<RoleDto>> _getRole(User user)
    {
        var result = new List<RoleDto>();
        if (user.UserRoles.Any())
        {
            foreach (var userRole in user.UserRoles)
            {
                var roleDto = new RoleDto();
                roleDto.RoleName = userRole.Role.Name;
                foreach (var roleClaim in userRole.Role.RolePrivileges)
                {
                    roleDto.Privileges.Add(new Dtos.PrivilegeDto
                    {
                        Id = roleClaim.Privilege.Id,
                        Action = roleClaim.Privilege.Action
                    });

                }
                result.Add(roleDto);

            }
        }

        return result;
    }
    private static List<Claim> _getClaim(UserLoginDto user)
    {
        var result = new List<Claim>();
        result.Add(new Claim("userName", user.Username));
        result.Add(new Claim("id", user.Id.ToString()));
        foreach (var role in user.Roles)
        {
            foreach (var clientClaim in role.Privileges)
            {
                Claim claim = new Claim(clientClaim.Id.ToString(), String.Format("{0}", clientClaim.Action));
                result.Add(claim);
            }
        }
        return result;
    }
    public string GetJwtString(int tokenLife, List<Claim> claimList)
    {
        var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             }
            .Union(claimList).ToList();
        var token = _tokenService.CreateSecurityToken(claims, tokenLife);
        return _tokenService.WriteToken(token);
    }
    private string _setRefreshToken(User user)
    {
        user.RefreshTokenExpireDate = DateTime.UtcNow.AddMinutes(user.userTokenLifetime);
        user.RefreshToken = _tokenService.GenerateRefreshToken();
        _context.Users.Update(user);
        _context.SaveChanges();
        return user.RefreshToken;
    }

}



