using CustomCodeFramework.Redis.Streams.Consuming;

namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamConsumer
{
    Task<RedisStreamConsumerResult> ConsumeAsync(
        RedisStreamConsumerContext context,
        CancellationToken cancellationToken = default
    );
}
