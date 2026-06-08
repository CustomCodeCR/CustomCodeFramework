using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomCodeFramework.Auth.Jwt;

public static class JwtExtensions
{
    public static AuthenticationBuilder AddCustomCodeJwtBearer(this AuthenticationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddJwtBearer(options =>
        {
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var jwtOptions = serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

            options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
                ),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
        });
    }
}
