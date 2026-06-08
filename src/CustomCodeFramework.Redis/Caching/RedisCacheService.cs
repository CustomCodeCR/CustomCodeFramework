using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Options;
using CustomCodeFramework.Redis.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Redis.Caching;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    ICacheSerializer serializer,
    IOptions<RedisOptions> options
) : ICacheService
{
    public async Task<TValue?> GetAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cachedValue = await distributedCache.GetStringAsync(key, cancellationToken);

        if (cachedValue is null)
        {
            return default;
        }

        return serializer.Deserialize<TValue>(cachedValue);
    }

    public async Task SetAsync<TValue>(
        string key,
        TValue value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var serializedValue = serializer.Serialize(value);

        await distributedCache.SetStringAsync(
            key,
            serializedValue,
            ToDistributedCacheEntryOptions(options),
            cancellationToken
        );
    }

    public async Task<TValue> GetOrSetAsync<TValue>(
        string key,
        Func<CancellationToken, Task<TValue>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cachedValue = await GetAsync<TValue>(key, cancellationToken);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory(cancellationToken);

        await SetAsync(key, value, options, cancellationToken);

        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = await distributedCache.GetStringAsync(key, cancellationToken);

        return value is not null;
    }

    private DistributedCacheEntryOptions ToDistributedCacheEntryOptions(
        CacheEntryOptions? cacheEntryOptions
    )
    {
        var defaultExpiration = TimeSpan.FromMinutes(options.Value.DefaultExpirationMinutes);

        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                cacheEntryOptions?.AbsoluteExpirationRelativeToNow ?? defaultExpiration,

            SlidingExpiration = cacheEntryOptions?.SlidingExpiration,
        };
    }
}
