using CustomCodeFramework.Redis.Streams.Messages;

namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamPublisher
{
    Task<string> PublishAsync(
        RedisStreamMessage message,
        CancellationToken cancellationToken = default
    );
}
