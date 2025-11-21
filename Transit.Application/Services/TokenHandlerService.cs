using Transit.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Transit.Application.Services;

public class TokenHandlerService
{
    private readonly JwtSettings _jwtSettings;
    private readonly byte[] _key;
    public TokenHandlerService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
        _key = Encoding.ASCII.GetBytes(_jwtSettings.SigningKey);
    }
    public JwtSecurityTokenHandler TokenHandler = new();
    public SecurityToken CreateSecurityToken(List<Claim> claims, int accessTokenLifetime)
    {
        var tokenDescriptor = GetTokenDescriptor(claims, accessTokenLifetime);

        return tokenDescriptor;
    }
    public string WriteToken(SecurityToken token)
    {
        return TokenHandler.WriteToken(token);
    }
    public bool ValidateToken(string claim, List<Claim> claims) => claims.Where(x => x.Value.ToLower() == claim.ToLower()).Any();
    public bool ValidateToken(string token)
    {
        SecurityToken securityToken = null;
        var validations = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        try
        {
            TokenHandler.ValidateToken(token, validations, out securityToken);
        }
        catch (Exception)
        {
            return false;
        }
        if (securityToken.ValidTo != DateTime.MinValue && securityToken.ValidTo > DateTime.UtcNow)
            return true;
        return false;
    }
    public List<Claim> GetClaims(string token)
    {
        var claims = new List<Claim>();
        try
        {
            var securityToken = TokenHandler.ReadJwtToken(token);
            claims = securityToken.Claims.ToList();
        }
        catch (Exception)
        {
            claims = new List<Claim>();
        }

        return claims;
    }
    private JwtSecurityToken GetTokenDescriptor(List<Claim> claims, int accessTokenLifetime)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(_key);
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: "InvoiceAutomation",
            claims: claims,
            expires: DateTime.Now.AddMinutes(accessTokenLifetime),
            signingCredentials: signingCredentials
            );
        return jwtSecurityToken;
    }
    public string GenerateRefreshToken()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 50)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public List<Claim> GetClaimFromRole(List<RoleDto> roles, string username, long userId)
    {
        var result = new List<Claim>();
        result.Add(new Claim("userName", username));
        result.Add(new Claim("id", userId.ToString()));

        foreach (var role in roles)
        {
            foreach (var userPrivilege in role.Privileges)
            {
                Claim claim = new Claim(userPrivilege.Id.ToString(), userPrivilege.Action);
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
        var token = CreateSecurityToken(claims, tokenLife);
        return WriteToken(token);
    }

}
