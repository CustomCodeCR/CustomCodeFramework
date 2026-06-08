using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Sms.Abstractions;
using CustomCodeFramework.Notifications.Sms.Channels;
using CustomCodeFramework.Notifications.Sms.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Notifications.Sms.DependencyInjection;

public static class SmsNotificationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeSmsNotifications(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<SmsNotificationOptions>()
            .Bind(configuration.GetSection(SmsNotificationOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultCountryCode),
                "SMS default country code is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ProviderName),
                "SMS provider name is required."
            )
            .ValidateOnStart();

        services.AddScoped<ISmsNotificationSender, NullSmsNotificationSender>();
        services.AddScoped<INotificationChannel, SmsNotificationChannel>();

        return services;
    }

    public static IServiceCollection AddSmsNotificationSender<TSender>(
        this IServiceCollection services
    )
        where TSender : class, ISmsNotificationSender
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ISmsNotificationSender, TSender>();

        return services;
    }
}
