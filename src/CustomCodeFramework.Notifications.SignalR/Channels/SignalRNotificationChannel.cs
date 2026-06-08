using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;
using CustomCodeFramework.Notifications.SignalR.Abstractions;
using CustomCodeFramework.Notifications.SignalR.Messages;

namespace CustomCodeFramework.Notifications.SignalR.Channels;

public sealed class SignalRNotificationChannel(
    ISignalRNotificationSender sender,
    SignalRNotificationMessageMapper mapper
) : INotificationChannel
{
    public NotificationChannelType ChannelType => NotificationChannelType.SignalR;

    public Task<NotificationChannelResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var signalRMessage = mapper.Map(message);

        return sender.SendAsync(signalRMessage, cancellationToken);
    }
}
