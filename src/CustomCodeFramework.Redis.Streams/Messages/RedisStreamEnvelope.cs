namespace CustomCodeFramework.Redis.Streams.Messages;

public sealed class RedisStreamEnvelope
{
    public string StreamName { get; init; } = default!;

    public string MessageId { get; init; } = default!;

    public string MessageType { get; init; } = default!;

    public string PayloadJson { get; init; } = default!;

    public Dictionary<string, string> Headers { get; init; } = [];

    public DateTime ReceivedAtUtc { get; init; } = DateTime.UtcNow;
}
