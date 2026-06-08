using System.Security.Claims;
using CustomCodeFramework.Auth.Claims;
using CustomCodeFramework.Notifications.SignalR.Abstractions;

namespace CustomCodeFramework.Notifications.SignalR.Channels;

public sealed class DefaultSignalRUserResolver : ISignalRUserResolver
{
    public string? ResolveUserId(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.GetUserId();
    }
}
