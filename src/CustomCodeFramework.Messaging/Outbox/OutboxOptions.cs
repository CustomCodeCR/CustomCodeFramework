namespace CustomCodeFramework.Messaging.Outbox;

public sealed record OutboxOptions
{
    public const string SectionName = "Messaging:Outbox";

    public bool Enabled { get; init; } = true;

    public int BatchSize { get; init; } = 50;

    public int MaxRetryCount { get; init; } = 5;

    public int PollingIntervalSeconds { get; init; } = 10;
}
