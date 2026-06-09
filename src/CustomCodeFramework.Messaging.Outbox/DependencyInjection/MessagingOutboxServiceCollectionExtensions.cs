using CustomCodeFramework.Messaging.Outbox.BackgroundServices;
using CustomCodeFramework.Messaging.Outbox.Options;
using CustomCodeFramework.Messaging.Outbox.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Messaging.Outbox.DependencyInjection;

public static class MessagingOutboxServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeMessagingOutbox(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<OutboxBackgroundServiceOptions>(
            configuration.GetSection("Messaging:Outbox:BackgroundService")
        );

        services.Configure<InboxCleanupOptions>(
            configuration.GetSection("Messaging:Inbox:Cleanup")
        );

        return services;
    }

    public static IServiceCollection AddCustomCodeOutboxProcessor<TProcessor>(
        this IServiceCollection services
    )
        where TProcessor : class, IOutboxProcessor
    {
        services.AddScoped<IOutboxProcessor, TProcessor>();

        return services;
    }

    public static IServiceCollection AddCustomCodeInboxProcessor<TProcessor>(
        this IServiceCollection services
    )
        where TProcessor : class, IInboxProcessor
    {
        services.AddScoped<IInboxProcessor, TProcessor>();

        return services;
    }

    public static IServiceCollection AddCustomCodeOutboxBackgroundService(
        this IServiceCollection services
    )
    {
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }

    public static IServiceCollection AddCustomCodeInboxCleanupBackgroundService(
        this IServiceCollection services
    )
    {
        services.AddHostedService<InboxCleanupBackgroundService>();

        return services;
    }

    public static IServiceCollection AddCustomCodeMessagingOutboxHostedServices(
        this IServiceCollection services
    )
    {
        services.AddHostedService<OutboxBackgroundService>();
        services.AddHostedService<InboxCleanupBackgroundService>();

        return services;
    }
}
