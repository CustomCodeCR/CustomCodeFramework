namespace CustomCodeFramework.Auth.Redis.Options;

public sealed class AuthRedisOptions
{
    public const string SectionName = "Auth:Redis";

    public string RevokedTokenKeyPrefix { get; init; } = "auth:revoked_tokens";

    public string ActiveSessionKeyPrefix { get; init; } = "auth:active_sessions";

    public int DefaultRevocationExpirationMinutes { get; init; } = 1440;

    public int DefaultSessionExpirationMinutes { get; init; } = 1440;

    public bool ValidateRevokedTokens { get; init; } = true;

    public bool ValidateActiveSessions { get; init; } = false;
}
