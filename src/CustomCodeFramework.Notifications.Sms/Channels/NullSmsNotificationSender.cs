using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Sms.Abstractions;
using CustomCodeFramework.Notifications.Sms.Messages;
using CustomCodeFramework.Notifications.Sms.Options;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.Sms.Channels;

public sealed class NullSmsNotificationSender(IOptions<SmsNotificationOptions> options)
    : ISmsNotificationSender
{
    public Task<NotificationChannelResult> SendAsync(
        SmsNotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!options.Value.Enabled)
        {
            return Task.FromResult(
                NotificationChannelResult.Failed("sms.disabled", "SMS notifications are disabled.")
            );
        }

        return Task.FromResult(
            NotificationChannelResult.Failed(
                "sms.provider_not_configured",
                "SMS provider is not configured."
            )
        );
    }
}
