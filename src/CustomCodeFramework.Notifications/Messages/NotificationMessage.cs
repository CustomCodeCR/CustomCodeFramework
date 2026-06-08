using CustomCodeFramework.Notifications.Channels;

namespace CustomCodeFramework.Notifications.Messages;

public sealed record NotificationMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public required NotificationChannelType Channel { get; init; }

    public required NotificationRecipient Recipient { get; init; }

    public string? Subject { get; init; }

    public required string Body { get; init; }

    public string? TemplateKey { get; init; }

    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;

    public IReadOnlyCollection<NotificationAttachment> Attachments { get; init; } = [];

    public IReadOnlyDictionary<string, object?> Data { get; init; } =
        new Dictionary<string, object?>();

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
