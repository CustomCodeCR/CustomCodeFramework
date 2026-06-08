using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.SignalR.Abstractions;
using CustomCodeFramework.Notifications.SignalR.Hubs;
using CustomCodeFramework.Notifications.SignalR.Messages;
using Microsoft.AspNetCore.SignalR;

namespace CustomCodeFramework.Notifications.SignalR.Channels;

public sealed class SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
    : ISignalRNotificationSender
{
    public async Task<NotificationChannelResult> SendAsync(
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            if (message.Target.Broadcast)
            {
                await hubContext.Clients.All.SendAsync(
                    message.ClientMethod,
                    message.Payload,
                    cancellationToken
                );

                return NotificationChannelResult.Success(message.MessageId.ToString("N"));
            }

            if (!string.IsNullOrWhiteSpace(message.Target.ConnectionId))
            {
                await hubContext
                    .Clients.Client(message.Target.ConnectionId)
                    .SendAsync(message.ClientMethod, message.Payload, cancellationToken);

                return NotificationChannelResult.Success(message.MessageId.ToString("N"));
            }

            if (!string.IsNullOrWhiteSpace(message.Target.GroupName))
            {
                await hubContext
                    .Clients.Group(message.Target.GroupName)
                    .SendAsync(message.ClientMethod, message.Payload, cancellationToken);

                return NotificationChannelResult.Success(message.MessageId.ToString("N"));
            }

            if (!string.IsNullOrWhiteSpace(message.Target.UserId))
            {
                await hubContext
                    .Clients.User(message.Target.UserId)
                    .SendAsync(message.ClientMethod, message.Payload, cancellationToken);

                return NotificationChannelResult.Success(message.MessageId.ToString("N"));
            }

            return NotificationChannelResult.Failed(
                "signalr.target_not_found",
                "SignalR notification target was not found."
            );
        }
        catch (Exception exception)
        {
            return NotificationChannelResult.Failed(
                "signalr.send_failed",
                exception.Message,
                exception
            );
        }
    }
}
