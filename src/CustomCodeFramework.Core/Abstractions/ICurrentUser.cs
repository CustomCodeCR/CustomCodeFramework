namespace CustomCodeFramework.Core.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? UserName { get; }

    string? Email { get; }

    string? UserType { get; }

    string? SessionId { get; }

    int? TokenVersion { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    IReadOnlyCollection<string> Scopes { get; }

    bool HasPermission(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    bool HasScope(string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        return Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
    }
}
