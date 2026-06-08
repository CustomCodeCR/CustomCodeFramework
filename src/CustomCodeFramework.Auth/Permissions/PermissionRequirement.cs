using Microsoft.AspNetCore.Authorization;

namespace CustomCodeFramework.Auth.Permissions;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
