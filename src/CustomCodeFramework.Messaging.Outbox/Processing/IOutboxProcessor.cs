namespace CustomCodeFramework.Messaging.Outbox.Processing;

public interface IOutboxProcessor
{
    Task<OutboxProcessingResult> ProcessAsync(
        int batchSize,
        CancellationToken cancellationToken = default
    );
}
