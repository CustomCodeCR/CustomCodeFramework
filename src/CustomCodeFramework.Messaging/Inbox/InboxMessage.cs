namespace CustomCodeFramework.Messaging.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid EventId { get; init; }

    public string EventType { get; init; } = string.Empty;

    public string EventName { get; init; } = string.Empty;

    public string SourceService { get; init; } = string.Empty;

    public string ConsumerService { get; init; } = string.Empty;

    public string? CorrelationId { get; init; }

    public InboxMessageStatus Status { get; set; } = InboxMessageStatus.Pending;

    public DateTime? ProcessedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
