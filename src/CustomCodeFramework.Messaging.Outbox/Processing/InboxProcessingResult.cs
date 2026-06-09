namespace CustomCodeFramework.Messaging.Outbox.Processing;

public sealed record InboxProcessingResult(int DeletedCount, bool HasMoreMessages)
{
    public static InboxProcessingResult Empty { get; } =
        new(DeletedCount: 0, HasMoreMessages: false);
}
