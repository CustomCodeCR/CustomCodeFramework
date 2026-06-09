namespace CustomCodeFramework.Workers.Options;

public sealed class WorkerOptions
{
    public bool Enabled { get; set; } = true;

    public int MaxRetryCount { get; set; } = 3;

    public int RetryDelaySeconds { get; set; } = 5;

    public bool UseDistributedLock { get; set; }

    public int LockDurationSeconds { get; set; } = 60;

    public bool StopOnUnhandledException { get; set; }
}
