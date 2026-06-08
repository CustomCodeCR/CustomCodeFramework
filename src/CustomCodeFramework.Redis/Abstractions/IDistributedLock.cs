using CustomCodeFramework.Redis.Locks;

namespace CustomCodeFramework.Redis.Abstractions;

public interface IDistributedLock
{
    Task<DistributedLockHandle?> AcquireAsync(
        string key,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );
}
