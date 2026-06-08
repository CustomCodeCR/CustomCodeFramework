using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Messaging.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Messaging.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .Validate(
                options => options.BatchSize > 0,
                "Outbox batch size must be greater than zero."
            )
            .Validate(
                options => options.MaxRetryCount >= 0,
                "Outbox max retry count cannot be negative."
            )
            .Validate(
                options => options.PollingIntervalSeconds > 0,
                "Outbox polling interval must be greater than zero."
            )
            .ValidateOnStart();

        services
            .AddOptions<InboxOptions>()
            .Bind(configuration.GetSection(InboxOptions.SectionName))
            .Validate(
                options => options.ProcessedMessageExpirationDays > 0,
                "Inbox expiration days must be greater than zero."
            )
            .ValidateOnStart();

        services.AddSingleton<IMessageSerializer, SystemTextJsonMessageSerializer>();

        return services;
    }
}
