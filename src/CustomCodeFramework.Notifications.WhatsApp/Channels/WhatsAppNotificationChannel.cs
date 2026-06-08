using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;
using CustomCodeFramework.Notifications.WhatsApp.Abstractions;
using CustomCodeFramework.Notifications.WhatsApp.Messages;

namespace CustomCodeFramework.Notifications.WhatsApp.Channels;

public sealed class WhatsAppNotificationChannel(
    IWhatsAppNotificationSender sender,
    WhatsAppNotificationMessageMapper mapper
) : INotificationChannel
{
    public NotificationChannelType ChannelType => NotificationChannelType.WhatsApp;

    public Task<NotificationChannelResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var whatsAppMessage = mapper.Map(message);

        return sender.SendAsync(whatsAppMessage, cancellationToken);
    }
}
