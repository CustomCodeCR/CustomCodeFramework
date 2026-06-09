namespace CustomCodeFramework.Messaging.Outbox.Processing;

public sealed record OutboxProcessingResult(
    int ProcessedCount,
    int FailedCount,
    bool HasMoreMessages
)
{
    public static OutboxProcessingResult Empty { get; } =
        new(ProcessedCount: 0, FailedCount: 0, HasMoreMessages: false);
}
