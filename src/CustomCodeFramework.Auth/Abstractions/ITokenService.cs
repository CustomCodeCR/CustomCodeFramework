namespace CustomCodeFramework.Auth.Abstractions;

public interface ITokenService
{
    string CreateToken(TokenRequest request);
}

public sealed record TokenRequest
{
    public required string UserId { get; init; }

    public string? UserName { get; init; }

    public string? Email { get; init; }

    public string? UserType { get; init; }

    public string? TenantId { get; init; }

    public string? SessionId { get; init; }

    public int? TokenVersion { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public IReadOnlyCollection<string> Permissions { get; init; } = [];

    public IReadOnlyCollection<string> Scopes { get; init; } = [];

    public IReadOnlyDictionary<string, string> ExtraClaims { get; init; } =
        new Dictionary<string, string>();
}
