using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Sms.Messages;

namespace CustomCodeFramework.Notifications.Sms.Abstractions;

public interface ISmsNotificationSender
{
    Task<NotificationChannelResult> SendAsync(
        SmsNotificationMessage message,
        CancellationToken cancellationToken = default
    );
}
