using CustomCodeFramework.Auth.Abstractions;
using CustomCodeFramework.Auth.ApiKeys;
using CustomCodeFramework.Auth.Jwt;
using CustomCodeFramework.Auth.Permissions;
using CustomCodeFramework.Auth.Scopes;
using CustomCodeFramework.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Auth.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        bool addJwt = true,
        bool addApiKeys = false
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "JWT issuer is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "JWT audience is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SecretKey),
                "JWT secret key is required."
            )
            .Validate(
                options => options.ExpirationMinutes > 0,
                "JWT expiration must be greater than zero."
            )
            .ValidateOnStart();

        services
            .AddOptions<ApiKeyOptions>()
            .Bind(configuration.GetSection(ApiKeyOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.HeaderName),
                "API key header name is required."
            )
            .ValidateOnStart();

        var authenticationBuilder = services.AddAuthentication();

        if (addJwt)
        {
            authenticationBuilder.AddCustomCodeJwtBearer();
        }

        if (addApiKeys)
        {
            authenticationBuilder.AddScheme<ApiKeyOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName,
                _ => { }
            );
        }

        services.AddAuthorization();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentUser>(provider =>
            provider.GetRequiredService<ICurrentUserService>()
        );

        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ScopeAuthorizationHandler>();

        return services;
    }
}
