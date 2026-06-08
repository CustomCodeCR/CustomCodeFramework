using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Delivery;

public sealed class NotificationDispatcher(IEnumerable<INotificationChannel> channels)
    : INotificationDispatcher
{
    public async Task<NotificationDeliveryResult> DispatchAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var channel = channels.FirstOrDefault(current => current.ChannelType == message.Channel);

        if (channel is null)
        {
            return NotificationDeliveryResult.Failed(
                message.MessageId,
                message.Channel,
                "notification.channel_not_registered",
                $"Notification channel '{message.Channel}' is not registered."
            );
        }

        var result = await channel.SendAsync(message, cancellationToken);

        if (result.IsSuccess)
        {
            return NotificationDeliveryResult.Sent(
                message.MessageId,
                message.Channel,
                result.ProviderMessageId
            );
        }

        return NotificationDeliveryResult.Failed(
            message.MessageId,
            message.Channel,
            result.Failure?.Code ?? "notification.channel_failed",
            result.Failure?.Message ?? "Notification channel failed."
        );
    }

    public async Task<IReadOnlyCollection<NotificationDeliveryResult>> DispatchAsync(
        IReadOnlyCollection<NotificationMessage> messages,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        var results = new List<NotificationDeliveryResult>();

        foreach (var message in messages)
        {
            var result = await DispatchAsync(message, cancellationToken);
            results.Add(result);
        }

        return results;
    }
}
