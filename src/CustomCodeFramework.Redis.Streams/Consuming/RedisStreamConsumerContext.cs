namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed class RedisStreamConsumerContext
{
    public string StreamName { get; init; } = default!;

    public string ConsumerGroup { get; init; } = default!;

    public string ConsumerName { get; init; } = default!;

    public int BatchSize { get; init; } = 10;

    public bool ReadPendingMessages { get; init; } = true;
}
