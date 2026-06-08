using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;
using CustomCodeFramework.Notifications.Sms.Abstractions;
using CustomCodeFramework.Notifications.Sms.Messages;

namespace CustomCodeFramework.Notifications.Sms.Channels;

public sealed class SmsNotificationChannel(ISmsNotificationSender sender) : INotificationChannel
{
    public NotificationChannelType ChannelType => NotificationChannelType.Sms;

    public Task<NotificationChannelResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var smsMessage = SmsNotificationMessageMapper.Map(message);

        return sender.SendAsync(smsMessage, cancellationToken);
    }
}
