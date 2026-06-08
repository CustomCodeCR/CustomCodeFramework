using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Email.Abstractions;
using CustomCodeFramework.Notifications.Email.Messages;
using CustomCodeFramework.Notifications.Email.Options;
using CustomCodeFramework.Notifications.Messages;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.Email.Channels;

public sealed class EmailNotificationChannel(
    IEmailNotificationSender sender,
    IOptions<EmailNotificationOptions> options
) : INotificationChannel
{
    public NotificationChannelType ChannelType => NotificationChannelType.Email;

    public Task<NotificationChannelResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var defaultFrom = new EmailAddress
        {
            Address = options.Value.FromEmail,
            Name = options.Value.FromName,
        };

        var emailMessage = EmailNotificationMessageMapper.Map(message, defaultFrom);

        return sender.SendAsync(emailMessage, cancellationToken);
    }
}
