using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomCodeFramework.Auth.Abstractions;
using CustomCodeFramework.Auth.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomCodeFramework.Auth.Jwt;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public string CreateToken(TokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        var jwtOptions = options.Value;
        var jti = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(CustomClaimTypes.UserId, request.UserId),
            new(JwtRegisteredClaimNames.Jti, jti),
        };

        AddIfNotEmpty(claims, CustomClaimTypes.UserName, request.UserName);
        AddIfNotEmpty(claims, ClaimTypes.Name, request.UserName);
        AddIfNotEmpty(claims, CustomClaimTypes.Email, request.Email);
        AddIfNotEmpty(claims, ClaimTypes.Email, request.Email);
        AddIfNotEmpty(claims, CustomClaimTypes.UserType, request.UserType);
        AddIfNotEmpty(claims, CustomClaimTypes.TenantId, request.TenantId);
        AddIfNotEmpty(claims, CustomClaimTypes.SessionId, request.SessionId);

        if (request.TokenVersion is not null)
        {
            claims.Add(new Claim(CustomClaimTypes.TokenVersion, request.TokenVersion.Value.ToString()));
        }

        foreach (var role in Clean(request.Roles))
        {
            claims.Add(new Claim(CustomClaimTypes.Role, role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in Clean(request.Permissions))
        {
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));
        }

        foreach (var scope in Clean(request.Scopes))
        {
            claims.Add(new Claim(CustomClaimTypes.Scope, scope));
        }

        foreach (var extraClaim in request.ExtraClaims)
        {
            if (!string.IsNullOrWhiteSpace(extraClaim.Key) && !string.IsNullOrWhiteSpace(extraClaim.Value))
            {
                claims.Add(new Claim(extraClaim.Key, extraClaim.Value));
            }
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void AddIfNotEmpty(ICollection<Claim> claims, string type, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type, value));
        }
    }

    private static IEnumerable<string> Clean(IEnumerable<string> values)
    {
        return values.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
