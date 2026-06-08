namespace CustomCodeFramework.Auth.Abstractions;

public interface IPermissionChecker
{
    bool HasPermission(string permission);

    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}
