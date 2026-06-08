namespace CustomCodeFramework.Auth.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "Auth:Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 60;

    public bool RequireHttpsMetadata { get; init; } = true;
}
