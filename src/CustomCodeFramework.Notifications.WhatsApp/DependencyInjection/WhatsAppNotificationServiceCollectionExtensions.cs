using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.WhatsApp.Abstractions;
using CustomCodeFramework.Notifications.WhatsApp.Channels;
using CustomCodeFramework.Notifications.WhatsApp.Messages;
using CustomCodeFramework.Notifications.WhatsApp.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Notifications.WhatsApp.DependencyInjection;

public static class WhatsAppNotificationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeWhatsAppNotifications(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<WhatsAppNotificationOptions>()
            .Bind(configuration.GetSection(WhatsAppNotificationOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultCountryCode),
                "WhatsApp default country code is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ProviderName),
                "WhatsApp provider name is required."
            )
            .ValidateOnStart();

        services.AddScoped<WhatsAppNotificationMessageMapper>();
        services.AddScoped<IWhatsAppNotificationSender, NullWhatsAppNotificationSender>();
        services.AddScoped<INotificationChannel, WhatsAppNotificationChannel>();

        return services;
    }

    public static IServiceCollection AddWhatsAppNotificationSender<TSender>(
        this IServiceCollection services
    )
        where TSender : class, IWhatsAppNotificationSender
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IWhatsAppNotificationSender, TSender>();

        return services;
    }
}
