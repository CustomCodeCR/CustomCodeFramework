namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed record RedisStreamConsumerResult(
    int ProcessedCount,
    int FailedCount,
    bool HasMessages
)
{
    public static RedisStreamConsumerResult Empty { get; } =
        new(ProcessedCount: 0, FailedCount: 0, HasMessages: false);
}
