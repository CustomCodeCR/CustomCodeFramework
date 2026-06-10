using CustomCodeFramework.Auth.Redis.Abstractions;
using CustomCodeFramework.Auth.Redis.Jwt;
using CustomCodeFramework.Auth.Redis.Options;
using CustomCodeFramework.Auth.Redis.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Auth.Redis.DependencyInjection;

public static class AuthRedisServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeAuthRedis(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AuthRedisOptions>()
            .Bind(configuration.GetSection(AuthRedisOptions.SectionName))
            .Validate(options => options.DefaultRevocationExpirationMinutes > 0, "Default revocation expiration must be greater than zero.")
            .Validate(options => options.DefaultSessionExpirationMinutes > 0, "Default session expiration must be greater than zero.")
            .ValidateOnStart();

        services.AddScoped<ITokenRevocationStore, RedisTokenRevocationStore>();
        services.AddScoped<ISessionValidationStore, RedisSessionValidationStore>();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, RedisJwtBearerEventsConfigurator>();

        return services;
    }
}
