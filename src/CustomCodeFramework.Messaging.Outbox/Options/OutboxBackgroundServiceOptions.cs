namespace CustomCodeFramework.Messaging.Outbox.Options;

public sealed class OutboxBackgroundServiceOptions
{
    public bool Enabled { get; set; } = true;

    public int BatchSize { get; set; } = 50;

    public int PollingIntervalSeconds { get; set; } = 10;

    public bool RunImmediately { get; set; } = true;

    public bool UseDistributedLock { get; set; } = true;

    public int LockDurationSeconds { get; set; } = 60;

    public int ErrorDelaySeconds { get; set; } = 10;
}
