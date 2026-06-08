using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Abstractions;

public interface INotificationRecipientResolver
{
    Task<IReadOnlyCollection<NotificationRecipient>> ResolveAsync(
        string recipientGroup,
        IReadOnlyDictionary<string, object?>? context = null,
        CancellationToken cancellationToken = default
    );
}
