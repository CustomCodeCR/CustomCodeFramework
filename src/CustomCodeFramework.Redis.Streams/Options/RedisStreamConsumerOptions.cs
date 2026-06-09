namespace CustomCodeFramework.Redis.Streams.Options;

public sealed class RedisStreamConsumerOptions
{
    public bool Enabled { get; set; } = true;

    public string StreamName { get; set; } = "events";

    public string ConsumerGroup { get; set; } = "default-group";

    public string ConsumerName { get; set; } = Environment.MachineName;

    public int BatchSize { get; set; } = 10;

    public int PollingIntervalSeconds { get; set; } = 5;

    public bool CreateConsumerGroupIfNotExists { get; set; } = true;

    public bool ReadPendingMessages { get; set; } = true;

    public int ErrorDelaySeconds { get; set; } = 10;
}
