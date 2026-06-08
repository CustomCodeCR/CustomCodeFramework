using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.Locks;

public sealed class RedisDistributedLock(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisOptions> options
) : IDistributedLock
{
    public async Task<DistributedLockHandle?> AcquireAsync(
        string key,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var database = connectionMultiplexer.GetDatabase();
        var value = Guid.NewGuid().ToString("N");
        var lockExpiration =
            expiration ?? TimeSpan.FromSeconds(options.Value.LockExpirationSeconds);

        var acquired = await database.StringSetAsync(key, value, lockExpiration, When.NotExists);

        return acquired ? new DistributedLockHandle(database, key, value) : null;
    }
}
