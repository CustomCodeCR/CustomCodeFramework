using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.SignalR.Abstractions;
using CustomCodeFramework.Notifications.SignalR.Channels;
using CustomCodeFramework.Notifications.SignalR.Hubs;
using CustomCodeFramework.Notifications.SignalR.Messages;
using CustomCodeFramework.Notifications.SignalR.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.SignalR.DependencyInjection;

public static class SignalRNotificationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeSignalRNotifications(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<SignalRNotificationOptions>()
            .Bind(configuration.GetSection(SignalRNotificationOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.HubPath),
                "SignalR hub path is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultClientMethod),
                "SignalR default client method is required."
            )
            .ValidateOnStart();

        var signalROptions =
            configuration
                .GetSection(SignalRNotificationOptions.SectionName)
                .Get<SignalRNotificationOptions>()
            ?? new SignalRNotificationOptions();

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = signalROptions.EnableDetailedErrors;
        });

        services.AddSingleton<ISignalRUserResolver, DefaultSignalRUserResolver>();
        services.AddScoped<SignalRNotificationMessageMapper>();
        services.AddScoped<ISignalRNotificationSender, SignalRNotificationSender>();
        services.AddScoped<INotificationChannel, SignalRNotificationChannel>();

        return services;
    }

    public static WebApplication MapCustomCodeNotificationHub(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.Services.GetRequiredService<IOptions<SignalRNotificationOptions>>().Value;

        app.MapHub<NotificationHub>(options.HubPath);

        return app;
    }
}
