using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Consuming;
using CustomCodeFramework.Redis.Streams.Options;
using CustomCodeFramework.Redis.Streams.Publishing;
using CustomCodeFramework.Redis.Streams.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Redis.Streams.DependencyInjection;

public static class RedisStreamsServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeRedisStreams(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<RedisStreamOptions>(configuration.GetSection("Redis:Streams"));

        services.Configure<RedisStreamConsumerOptions>(
            configuration.GetSection("Redis:Streams:Consumer")
        );

        services.Configure<RedisStreamPublisherOptions>(
            configuration.GetSection("Redis:Streams:Publisher")
        );

        services.AddSingleton<RedisStreamMessageSerializer>();
        services.AddScoped<IRedisStreamPublisher, RedisStreamPublisher>();
        services.AddScoped<IRedisStreamConsumer, RedisStreamConsumer>();

        return services;
    }

    public static IServiceCollection AddCustomCodeRedisStreamHandler<THandler>(
        this IServiceCollection services
    )
        where THandler : class, IRedisStreamMessageHandler
    {
        services.AddScoped<IRedisStreamMessageHandler, THandler>();

        return services;
    }

    public static IServiceCollection AddCustomCodeRedisStreamConsumerBackgroundService(
        this IServiceCollection services
    )
    {
        services.AddHostedService<RedisStreamConsumerBackgroundService>();

        return services;
    }
}
