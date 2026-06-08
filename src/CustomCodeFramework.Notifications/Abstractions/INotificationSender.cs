using CustomCodeFramework.Notifications.Delivery;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Abstractions;

public interface INotificationSender
{
    Task<NotificationDeliveryResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    );
}
