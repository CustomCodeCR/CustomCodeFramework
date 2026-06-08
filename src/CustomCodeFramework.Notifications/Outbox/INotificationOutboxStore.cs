namespace CustomCodeFramework.Notifications.Outbox;

public interface INotificationOutboxStore
{
    Task AddAsync(NotificationOutboxMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationOutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTime nowUtc,
        CancellationToken cancellationToken = default
    );

    Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAtUtc,
        CancellationToken cancellationToken = default
    );

    Task MarkAsFailedAsync(
        Guid id,
        string error,
        DateTime nextAttemptAtUtc,
        CancellationToken cancellationToken = default
    );
}
