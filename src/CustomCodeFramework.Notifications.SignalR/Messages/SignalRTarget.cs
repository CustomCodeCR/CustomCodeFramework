namespace CustomCodeFramework.Notifications.SignalR.Messages;

public sealed record SignalRTarget
{
    public string? UserId { get; init; }

    public string? GroupName { get; init; }

    public string? ConnectionId { get; init; }

    public bool Broadcast { get; init; }

    public static SignalRTarget User(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return new SignalRTarget { UserId = userId };
    }

    public static SignalRTarget Group(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        return new SignalRTarget { GroupName = groupName };
    }

    public static SignalRTarget Connection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        return new SignalRTarget { ConnectionId = connectionId };
    }

    public static SignalRTarget All()
    {
        return new SignalRTarget { Broadcast = true };
    }
}
