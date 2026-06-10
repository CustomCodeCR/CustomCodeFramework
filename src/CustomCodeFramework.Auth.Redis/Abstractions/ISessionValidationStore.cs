namespace CustomCodeFramework.Auth.Redis.Abstractions;

public interface ISessionValidationStore
{
    Task MarkActiveAsync(string sessionId, string userId, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(string sessionId, string userId, CancellationToken cancellationToken = default);

    Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default);
}
