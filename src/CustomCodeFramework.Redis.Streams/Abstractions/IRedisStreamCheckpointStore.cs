namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamCheckpointStore
{
    Task<string?> GetLastMessageIdAsync(
        string streamName,
        string consumerGroup,
        CancellationToken cancellationToken = default
    );

    Task SaveLastMessageIdAsync(
        string streamName,
        string consumerGroup,
        string messageId,
        CancellationToken cancellationToken = default
    );
}
