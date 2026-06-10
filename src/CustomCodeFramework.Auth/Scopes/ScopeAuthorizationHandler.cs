using CustomCodeFramework.Auth.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CustomCodeFramework.Auth.Scopes;

public sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement
    )
    {
        if (context.User.Identity?.IsAuthenticated == true && context.User.HasScope(requirement.Scope))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
