namespace CustomCodeFramework.Workers.Scheduling;

public sealed class WorkerTimer : IAsyncDisposable
{
    private readonly PeriodicTimer _timer;

    public WorkerTimer(TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        _timer = new PeriodicTimer(interval);
    }

    public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
    {
        return _timer.WaitForNextTickAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _timer.Dispose();

        return ValueTask.CompletedTask;
    }
}
