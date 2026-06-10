using System.IdentityModel.Tokens.Jwt;
using CustomCodeFramework.Auth.Claims;
using CustomCodeFramework.Auth.Redis.Abstractions;
using CustomCodeFramework.Auth.Redis.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Auth.Redis.Jwt;

internal sealed class RedisJwtBearerEventsConfigurator(IServiceProvider serviceProvider)
    : IPostConfigureOptions<JwtBearerOptions>
{
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var previous = options.Events;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                if (previous.OnTokenValidated is not null)
                {
                    await previous.OnTokenValidated(context);
                }

                var authRedisOptions = context.HttpContext.RequestServices
                    .GetRequiredService<IOptions<AuthRedisOptions>>()
                    .Value;

                if (authRedisOptions.ValidateRevokedTokens)
                {
                    var tokenId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    if (!string.IsNullOrWhiteSpace(tokenId))
                    {
                        var store = context.HttpContext.RequestServices.GetRequiredService<ITokenRevocationStore>();
                        if (await store.IsRevokedAsync(tokenId, context.HttpContext.RequestAborted))
                        {
                            context.Fail("Token has been revoked.");
                            return;
                        }
                    }
                }

                if (authRedisOptions.ValidateActiveSessions)
                {
                    var userId = context.Principal?.GetUserId();
                    var sessionId = context.Principal?.GetSessionId();

                    if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId))
                    {
                        context.Fail("Active session is required.");
                        return;
                    }

                    var sessions = context.HttpContext.RequestServices.GetRequiredService<ISessionValidationStore>();
                    if (!await sessions.IsActiveAsync(sessionId, userId, context.HttpContext.RequestAborted))
                    {
                        context.Fail("Session is not active.");
                    }
                }
            },
            OnAuthenticationFailed = previous.OnAuthenticationFailed,
            OnChallenge = previous.OnChallenge,
            OnForbidden = previous.OnForbidden,
            OnMessageReceived = previous.OnMessageReceived
        };
    }
}
