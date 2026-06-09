namespace CustomCodeFramework.Messaging.Outbox.Options;

public sealed class InboxCleanupOptions
{
    public bool Enabled { get; set; } = true;

    public int BatchSize { get; set; } = 100;

    public int PollingIntervalSeconds { get; set; } = 3600;

    public int DeleteMessagesOlderThanDays { get; set; } = 30;

    public bool RunImmediately { get; set; }

    public bool UseDistributedLock { get; set; } = true;

    public int LockDurationSeconds { get; set; } = 300;

    public int ErrorDelaySeconds { get; set; } = 30;
}
