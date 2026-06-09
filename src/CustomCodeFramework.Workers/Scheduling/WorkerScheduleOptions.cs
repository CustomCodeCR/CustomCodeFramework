namespace CustomCodeFramework.Workers.Scheduling;

public sealed class WorkerScheduleOptions
{
    public int IntervalSeconds { get; set; } = 30;

    public bool RunImmediately { get; set; } = true;

    public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);
}
