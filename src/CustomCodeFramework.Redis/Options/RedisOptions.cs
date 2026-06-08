namespace CustomCodeFramework.Redis.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;

    public string InstanceName { get; init; } = "customcode:";

    public bool EnableHealthCheck { get; init; } = true;

    public int DefaultExpirationMinutes { get; init; } = 30;

    public int LockExpirationSeconds { get; init; } = 30;
}
