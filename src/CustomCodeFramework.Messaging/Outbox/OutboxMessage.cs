namespace CustomCodeFramework.Messaging.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid EventId { get; init; }

    public string EventType { get; init; } = string.Empty;

    public string EventName { get; init; } = string.Empty;

    public string SourceService { get; init; } = string.Empty;

    public string PayloadJson { get; init; } = string.Empty;

    public string? HeadersJson { get; init; }

    public string? CorrelationId { get; init; }

    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

    public int RetryCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? ProcessedAtUtc { get; set; }
}
