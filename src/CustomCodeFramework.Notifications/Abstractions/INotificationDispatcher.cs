using CustomCodeFramework.Notifications.Delivery;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Abstractions;

public interface INotificationDispatcher
{
    Task<NotificationDeliveryResult> DispatchAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<NotificationDeliveryResult>> DispatchAsync(
        IReadOnlyCollection<NotificationMessage> messages,
        CancellationToken cancellationToken = default
    );
}
