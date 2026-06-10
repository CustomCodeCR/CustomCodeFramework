using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Auth.Redis.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Redis.Streams.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.ServiceDefaults.DependencyInjection;

public static class ServiceDefaultsServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<CustomCodeServiceDefaultsOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new CustomCodeServiceDefaultsOptions();
        configure?.Invoke(options);

        if (options.AddApi)
        {
            services.AddCustomCodeApiWithSwagger();
        }

        if (options.AddAuth)
        {
            services.AddCustomCodeAuth(configuration);
        }

        if (options.AddRedis)
        {
            services.AddCustomCodeRedis(configuration);
        }

        if (options.AddAuthRedis)
        {
            services.AddCustomCodeAuthRedis(configuration);
        }

        if (options.AddPostgres)
        {
            services.AddCustomCodePostgres(configuration);
        }

        if (options.AddMongo)
        {
            services.AddCustomCodeMongo(configuration);
        }

        if (options.AddRedisStreams)
        {
            services.AddCustomCodeRedisStreams(configuration);
        }

        services.AddHealthChecks();

        return services;
    }
}
