namespace CustomCodeFramework.Notifications.Sms.Messages;

public sealed record SmsNotificationMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public required PhoneNumber To { get; init; }

    public required string Body { get; init; }

    public IReadOnlyDictionary<string, object?> Data { get; init; } =
        new Dictionary<string, object?>();

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
