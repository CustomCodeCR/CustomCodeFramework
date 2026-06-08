using System.Security.Claims;

namespace CustomCodeFramework.Notifications.SignalR.Abstractions;

public interface ISignalRUserResolver
{
    string? ResolveUserId(ClaimsPrincipal user);
}
