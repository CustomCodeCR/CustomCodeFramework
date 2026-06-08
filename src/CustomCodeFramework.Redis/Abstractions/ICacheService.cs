using CustomCodeFramework.Redis.Caching;

namespace CustomCodeFramework.Redis.Abstractions;

public interface ICacheService
{
    Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<TValue>(
        string key,
        TValue value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default
    );

    Task<TValue> GetOrSetAsync<TValue>(
        string key,
        Func<CancellationToken, Task<TValue>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
