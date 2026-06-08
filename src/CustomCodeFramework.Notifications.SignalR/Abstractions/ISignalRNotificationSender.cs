using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.SignalR.Messages;

namespace CustomCodeFramework.Notifications.SignalR.Abstractions;

public interface ISignalRNotificationSender
{
    Task<NotificationChannelResult> SendAsync(
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default
    );
}
