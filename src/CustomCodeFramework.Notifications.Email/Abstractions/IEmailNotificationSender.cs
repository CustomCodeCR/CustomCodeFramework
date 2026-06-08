using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Email.Messages;

namespace CustomCodeFramework.Notifications.Email.Abstractions;

public interface IEmailNotificationSender
{
    Task<NotificationChannelResult> SendAsync(
        EmailNotificationMessage message,
        CancellationToken cancellationToken = default
    );
}
