using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Abstractions;

public interface INotification
{
    Guid NotificationId { get; }

    NotificationChannelType Channel { get; }

    NotificationRecipient Recipient { get; }

    string? Subject { get; }

    string Body { get; }

    NotificationPriority Priority { get; }
}
