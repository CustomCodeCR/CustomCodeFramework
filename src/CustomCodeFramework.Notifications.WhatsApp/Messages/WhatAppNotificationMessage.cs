namespace CustomCodeFramework.Notifications.WhatsApp.Messages;

public sealed record WhatsAppNotificationMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public required WhatsAppPhoneNumber To { get; init; }

    public string? Body { get; init; }

    public WhatsAppTemplateMessage? Template { get; init; }

    public IReadOnlyDictionary<string, object?> Data { get; init; } =
        new Dictionary<string, object?>();

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public bool IsTemplate => Template is not null;
}
