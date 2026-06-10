using Microsoft.AspNetCore.Authorization;

namespace CustomCodeFramework.Auth.Scopes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireScopeAttribute(string scope) : AuthorizeAttribute(ScopePolicy.NamePrefix + scope)
{
    public string Scope { get; } = scope;
}
