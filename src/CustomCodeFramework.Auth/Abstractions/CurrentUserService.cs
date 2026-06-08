using System.Security.Claims;
using CustomCodeFramework.Auth.Claims;
using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Auth.Abstractions;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public string? UserId => User?.GetUserId();

    public string? UserName => User?.GetUserName();

    public string? Email => User?.GetEmail();

    public string? TenantId => User?.GetTenantId();

    public string? SessionId => User?.FindFirst(CustomClaimTypes.SessionId)?.Value;

    public IReadOnlyCollection<string> Roles => User?.GetRoles() ?? [];

    public IReadOnlyCollection<string> Permissions => User?.GetPermissions() ?? [];
}
