using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Abstractions;

public interface INotificationChannel
{
    NotificationChannelType ChannelType { get; }

    Task<NotificationChannelResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    );
}
