using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using CustomCodeFramework.Redis.HealthChecks;
using CustomCodeFramework.Redis.Locks;
using CustomCodeFramework.Redis.Options;
using CustomCodeFramework.Redis.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeRedis(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Redis connection string is required."
            )
            .Validate(
                options => options.DefaultExpirationMinutes > 0,
                "Redis default expiration must be greater than zero."
            )
            .Validate(
                options => options.LockExpirationSeconds > 0,
                "Redis lock expiration must be greater than zero."
            )
            .ValidateOnStart();

        var redisOptions =
            configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? new RedisOptions();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString)
        );

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisOptions.ConnectionString;
            options.InstanceName = redisOptions.InstanceName;
        });

        services.AddSingleton<ICacheSerializer, SystemTextJsonCacheSerializer>();
        services.AddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();

        if (redisOptions.EnableHealthCheck)
        {
            services
                .AddHealthChecks()
                .AddCheck<RedisHealthCheck>("redis", tags: ["cache", "redis"]);
        }

        return services;
    }
}
