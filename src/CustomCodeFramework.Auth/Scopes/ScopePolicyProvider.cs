using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Auth.Scopes;

public sealed class ScopePolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(ScopePolicy.NamePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var scope = policyName[ScopePolicy.NamePrefix.Length..];
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ScopeRequirement(scope))
                .Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}
