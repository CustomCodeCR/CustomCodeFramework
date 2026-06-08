using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.WhatsApp.Abstractions;
using CustomCodeFramework.Notifications.WhatsApp.Messages;
using CustomCodeFramework.Notifications.WhatsApp.Options;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.WhatsApp.Channels;

public sealed class NullWhatsAppNotificationSender(IOptions<WhatsAppNotificationOptions> options)
    : IWhatsAppNotificationSender
{
    public Task<NotificationChannelResult> SendAsync(
        WhatsAppNotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!options.Value.Enabled)
        {
            return Task.FromResult(
                NotificationChannelResult.Failed(
                    "whatsapp.disabled",
                    "WhatsApp notifications are disabled."
                )
            );
        }

        return Task.FromResult(
            NotificationChannelResult.Failed(
                "whatsapp.provider_not_configured",
                "WhatsApp provider is not configured."
            )
        );
    }
}
