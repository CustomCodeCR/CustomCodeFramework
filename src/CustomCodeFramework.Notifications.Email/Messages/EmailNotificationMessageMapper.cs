using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Email.Messages;

public static class EmailNotificationMessageMapper
{
    public static EmailNotificationMessage Map(
        NotificationMessage message,
        EmailAddress defaultFrom
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(defaultFrom);

        if (string.IsNullOrWhiteSpace(message.Recipient.Email))
        {
            throw new InvalidOperationException("Email recipient address is required.");
        }

        return new EmailNotificationMessage
        {
            MessageId = message.MessageId,
            From = defaultFrom,
            To = new EmailAddress
            {
                Address = message.Recipient.Email,
                Name = message.Recipient.Name,
            },
            Subject = message.Subject ?? string.Empty,
            Body = message.Body,
            IsHtml = true,
            Attachments = message
                .Attachments.Where(attachment => attachment.Content is not null)
                .Select(attachment => new EmailAttachment
                {
                    FileName = attachment.FileName,
                    ContentType = attachment.ContentType,
                    Content = attachment.Content!,
                })
                .ToArray(),
        };
    }
}
