using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Delivery;
using CustomCodeFramework.Notifications.Options;
using CustomCodeFramework.Notifications.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Notifications.DependencyInjection;

public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeNotifications(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<NotificationsOptions>()
            .Bind(configuration.GetSection(NotificationsOptions.SectionName))
            .Validate(
                options => options.OutboxBatchSize > 0,
                "Notification outbox batch size must be greater than zero."
            )
            .Validate(
                options => options.OutboxMaxRetryCount >= 0,
                "Notification outbox max retry count cannot be negative."
            )
            .Validate(
                options => options.OutboxRetryDelaySeconds > 0,
                "Notification outbox retry delay must be greater than zero."
            )
            .ValidateOnStart();

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddScoped<NotificationOutboxProcessor>();

        return services;
    }

    public static IServiceCollection AddNotificationChannel<TChannel>(
        this IServiceCollection services
    )
        where TChannel : class, INotificationChannel
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<INotificationChannel, TChannel>();

        return services;
    }
}
