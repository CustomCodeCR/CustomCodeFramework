using CustomCodeFramework.Auth.Abstractions;

namespace CustomCodeFramework.Auth.Permissions;

public sealed class PermissionChecker(ICurrentUserService currentUserService) : IPermissionChecker
{
    public bool HasPermission(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return currentUserService.Permissions.Contains(
            permission,
            StringComparer.OrdinalIgnoreCase
        );
    }

    public Task<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(HasPermission(permission));
    }
}
