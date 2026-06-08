using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Email.Abstractions;
using CustomCodeFramework.Notifications.Email.Channels;
using CustomCodeFramework.Notifications.Email.Options;
using CustomCodeFramework.Notifications.Email.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Notifications.Email.DependencyInjection;

public static class EmailNotificationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeEmailNotifications(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<EmailNotificationOptions>()
            .Bind(configuration.GetSection(EmailNotificationOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "Email SMTP host is required."
            )
            .Validate(options => options.Port > 0, "Email SMTP port must be greater than zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.FromEmail),
                "Email sender address is required."
            )
            .Validate(
                options => options.TimeoutSeconds > 0,
                "Email timeout must be greater than zero."
            )
            .ValidateOnStart();

        services.AddScoped<IEmailNotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();
        services.AddScoped<INotificationTemplateRenderer, EmailTemplateRenderer>();

        return services;
    }
}
