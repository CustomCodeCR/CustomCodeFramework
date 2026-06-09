namespace CustomCodeFramework.Redis.Streams.Options;

public sealed class RedisStreamOptions
{
    public bool Enabled { get; set; } = true;

    public string DefaultStreamName { get; set; } = "events";

    public string SourceService { get; set; } = "unknown-service";
}
