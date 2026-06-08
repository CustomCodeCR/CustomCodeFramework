using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.WhatsApp.Messages;

namespace CustomCodeFramework.Notifications.WhatsApp.Abstractions;

public interface IWhatsAppNotificationSender
{
    Task<NotificationChannelResult> SendAsync(
        WhatsAppNotificationMessage message,
        CancellationToken cancellationToken = default
    );
}
