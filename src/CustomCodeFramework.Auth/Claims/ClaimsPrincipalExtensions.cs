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

    public static string? GetUserType(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(CustomClaimTypes.UserType);
    }

    public static string? GetSessionId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(CustomClaimTypes.SessionId);
    }

    public static int? GetTokenVersion(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(CustomClaimTypes.TokenVersion);
        return int.TryParse(value, out var version) ? version : null;
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
            .SelectMany(SplitClaimValues)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyCollection<string> GetScopes(this ClaimsPrincipal principal)
    {
        return principal
            .FindAll(CustomClaimTypes.Scope)
            .Concat(principal.FindAll("scp"))
            .SelectMany(claim => SplitClaimValues(claim.Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return principal.GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public static bool HasScope(this ClaimsPrincipal principal, string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        return principal.GetScopes().Contains(scope, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SplitClaimValues(Claim claim) => SplitClaimValues(claim.Value);

    private static IEnumerable<string> SplitClaimValues(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
