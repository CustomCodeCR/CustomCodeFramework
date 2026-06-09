namespace CustomCodeFramework.Redis.Streams.Options;

public sealed class RedisStreamPublisherOptions
{
    public long MaxLength { get; set; } = 100_000;

    public bool UseApproximateMaxLength { get; set; } = true;
}
