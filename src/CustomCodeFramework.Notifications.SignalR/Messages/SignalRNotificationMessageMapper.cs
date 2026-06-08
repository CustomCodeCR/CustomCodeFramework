using CustomCodeFramework.Notifications.Messages;
using CustomCodeFramework.Notifications.SignalR.Options;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.SignalR.Messages;

public sealed class SignalRNotificationMessageMapper(IOptions<SignalRNotificationOptions> options)
{
    public SignalRNotificationMessage Map(NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new SignalRNotificationMessage
        {
            MessageId = message.MessageId,
            Target = ResolveTarget(message),
            ClientMethod = options.Value.DefaultClientMethod,
            Subject = message.Subject,
            Body = message.Body,
            Payload = new
            {
                message.MessageId,
                message.Subject,
                message.Body,
                message.Priority,
                message.Data,
                message.CreatedAtUtc,
            },
            CreatedAtUtc = message.CreatedAtUtc,
        };
    }

    private SignalRTarget ResolveTarget(NotificationMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.Recipient.UserId))
        {
            return SignalRTarget.User(message.Recipient.UserId);
        }

        if (
            message.Data.TryGetValue(options.Value.GroupMetadataKey, out var groupValue)
            && groupValue is not null
            && !string.IsNullOrWhiteSpace(groupValue.ToString())
        )
        {
            return SignalRTarget.Group(groupValue.ToString()!);
        }

        if (
            message.Data.TryGetValue("connectionId", out var connectionValue)
            && connectionValue is not null
            && !string.IsNullOrWhiteSpace(connectionValue.ToString())
        )
        {
            return SignalRTarget.Connection(connectionValue.ToString()!);
        }

        return SignalRTarget.All();
    }
}
