using CustomCodeFramework.Auth.Redis.Abstractions;
using CustomCodeFramework.Auth.Redis.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CustomCodeFramework.Auth.Redis.Stores;

public sealed class RedisSessionValidationStore(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<AuthRedisOptions> options
) : ISessionValidationStore
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly AuthRedisOptions _options = options.Value;

    public async Task MarkActiveAsync(string sessionId, string userId, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var ttl = expiration ?? TimeSpan.FromMinutes(_options.DefaultSessionExpirationMinutes);
        await _database.StringSetAsync(BuildKey(sessionId), userId, ttl).WaitAsync(cancellationToken);
    }

    public async Task<bool> IsActiveAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var storedUserId = await _database.StringGetAsync(BuildKey(sessionId)).WaitAsync(cancellationToken);
        return storedUserId.HasValue && string.Equals(storedUserId.ToString(), userId, StringComparison.OrdinalIgnoreCase);
    }

    public async Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        await _database.KeyDeleteAsync(BuildKey(sessionId)).WaitAsync(cancellationToken);
    }

    private RedisKey BuildKey(string sessionId) => $"{_options.ActiveSessionKeyPrefix}:{sessionId}";
}
