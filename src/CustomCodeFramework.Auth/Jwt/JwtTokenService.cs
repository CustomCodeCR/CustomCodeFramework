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

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(CustomClaimTypes.UserId, request.UserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            claims.Add(new Claim(CustomClaimTypes.UserName, request.UserName));
            claims.Add(new Claim(ClaimTypes.Name, request.UserName));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            claims.Add(new Claim(CustomClaimTypes.Email, request.Email));
            claims.Add(new Claim(ClaimTypes.Email, request.Email));
        }

        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            claims.Add(new Claim(CustomClaimTypes.TenantId, request.TenantId));
        }

        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            claims.Add(new Claim(CustomClaimTypes.SessionId, request.SessionId));
        }

        foreach (var role in request.Roles.Where(role => !string.IsNullOrWhiteSpace(role)))
        {
            claims.Add(new Claim(CustomClaimTypes.Role, role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (
            var permission in request.Permissions.Where(permission =>
                !string.IsNullOrWhiteSpace(permission)
            )
        )
        {
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));
        }

        foreach (var extraClaim in request.ExtraClaims)
        {
            if (
                !string.IsNullOrWhiteSpace(extraClaim.Key)
                && !string.IsNullOrWhiteSpace(extraClaim.Value)
            )
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
}
