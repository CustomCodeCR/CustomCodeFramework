using CustomCodeFramework.Redis.Streams.Messages;

namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamMessageHandler
{
    string MessageType { get; }

    Task HandleAsync(RedisStreamEnvelope envelope, CancellationToken cancellationToken = default);
}
