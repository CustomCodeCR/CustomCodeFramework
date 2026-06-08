using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Delivery;

public sealed class NotificationSender(INotificationDispatcher dispatcher) : INotificationSender
{
    public Task<NotificationDeliveryResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        return dispatcher.DispatchAsync(message, cancellationToken);
    }
}
