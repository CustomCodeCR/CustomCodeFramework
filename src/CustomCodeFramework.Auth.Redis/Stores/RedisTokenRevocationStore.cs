using CustomCodeFramework.Auth.Redis.Abstractions;
using CustomCodeFramework.Auth.Redis.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CustomCodeFramework.Auth.Redis.Stores;

public sealed class RedisTokenRevocationStore(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<AuthRedisOptions> options
) : ITokenRevocationStore
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly AuthRedisOptions _options = options.Value;

    public async Task RevokeAsync(string tokenId, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
        var ttl = expiration ?? TimeSpan.FromMinutes(_options.DefaultRevocationExpirationMinutes);
        await _database.StringSetAsync(BuildKey(tokenId), "1", ttl).WaitAsync(cancellationToken);
    }

    public async Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return false;
        }

        return await _database.KeyExistsAsync(BuildKey(tokenId)).WaitAsync(cancellationToken);
    }

    private RedisKey BuildKey(string tokenId) => $"{_options.RevokedTokenKeyPrefix}:{tokenId}";
}
