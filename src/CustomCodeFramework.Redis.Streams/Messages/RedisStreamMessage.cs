namespace CustomCodeFramework.Redis.Streams.Messages;

public sealed class RedisStreamMessage
{
    public string StreamName { get; init; } = string.Empty;

    public string MessageType { get; init; } = default!;

    public object Payload { get; init; } = default!;

    public Dictionary<string, string> Headers { get; init; } = [];
}
