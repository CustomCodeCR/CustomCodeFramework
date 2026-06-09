namespace CustomCodeFramework.Workers.Abstractions;

public interface IWorkerLockProvider
{
    Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );
}
