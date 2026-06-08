using CustomCodeFramework.Messaging.Envelopes;

namespace CustomCodeFramework.Messaging.Outbox;

public interface IOutboxStore
{
    Task AddAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EventEnvelope>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default
    );

    Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(
        Guid eventId,
        string error,
        CancellationToken cancellationToken = default
    );
}
