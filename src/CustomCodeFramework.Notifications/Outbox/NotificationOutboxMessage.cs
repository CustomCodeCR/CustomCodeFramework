using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Outbox;

public sealed record NotificationOutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required NotificationMessage Message { get; init; }

    public int AttemptCount { get; init; }

    public int MaxRetryCount { get; init; } = 5;

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? NextAttemptAtUtc { get; init; }

    public DateTime? ProcessedAtUtc { get; init; }

    public string? LastError { get; init; }

    public bool IsProcessed => ProcessedAtUtc is not null;
}
