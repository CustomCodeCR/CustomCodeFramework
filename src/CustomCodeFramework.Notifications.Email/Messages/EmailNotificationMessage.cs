namespace CustomCodeFramework.Notifications.Email.Messages;

public sealed record EmailNotificationMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public required EmailAddress To { get; init; }

    public EmailAddress? From { get; init; }

    public IReadOnlyCollection<EmailAddress> Cc { get; init; } = [];

    public IReadOnlyCollection<EmailAddress> Bcc { get; init; } = [];

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public bool IsHtml { get; init; } = true;

    public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = [];
}
