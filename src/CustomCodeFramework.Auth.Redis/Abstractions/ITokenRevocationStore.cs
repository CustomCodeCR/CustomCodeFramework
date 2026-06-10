namespace CustomCodeFramework.Auth.Redis.Abstractions;

public interface ITokenRevocationStore
{
    Task RevokeAsync(string tokenId, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
}
