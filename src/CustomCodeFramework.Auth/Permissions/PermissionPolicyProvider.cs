using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Auth.Permissions;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyPrefix = "Permission:";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existingPolicy = await base.GetPolicyAsync(policyName);

        if (existingPolicy is not null)
        {
            return existingPolicy;
        }

        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var permission = policyName[PolicyPrefix.Length..];

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }

    public static string BuildPolicyName(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return $"{PolicyPrefix}{permission}";
    }
}
