namespace CustomCodeFramework.Messaging.Outbox.Processing;

public interface IInboxProcessor
{
    Task<InboxProcessingResult> CleanupAsync(
        TimeSpan olderThan,
        int batchSize,
        CancellationToken cancellationToken = default
    );
}
