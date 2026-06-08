using CustomCodeFramework.Notifications.SignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace CustomCodeFramework.Notifications.SignalR.Hubs;

public sealed class NotificationHub(ISignalRUserResolver userResolver) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = userResolver.ResolveUserId(Context.User!);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        await base.OnConnectedAsync();
    }

    public Task JoinGroupAsync(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public Task LeaveGroupAsync(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
