using System.Security.Claims;

namespace CustomCodeFramework.Auth.Claims;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(CustomClaimTypes.UserId)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
    }

    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(CustomClaimTypes.UserName) ?? principal.Identity?.Name;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(CustomClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Email);
    }

    public static string? GetTenantId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(CustomClaimTypes.TenantId);
    }

    public static IReadOnlyCollection<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal
            .FindAll(CustomClaimTypes.Role)
            .Select(claim => claim.Value)
            .Concat(principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyCollection<string> GetPermissions(this ClaimsPrincipal principal)
    {
        return principal
            .FindAll(CustomClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return principal.GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
