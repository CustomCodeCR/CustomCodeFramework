using Microsoft.AspNetCore.Authorization;

namespace CustomCodeFramework.Auth.Scopes;

public sealed class ScopeRequirement(string scope) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
}
