namespace CustomCodeFramework.Postgres.Resilience;

public sealed class PostgresRetryOptions
{
    public const string SectionName = "Postgres:Retry";

    public int MaxRetryCount { get; init; } = 3;

    public int DelayMilliseconds { get; init; } = 200;

    public int MaxDelayMilliseconds { get; init; } = 2_000;
}
