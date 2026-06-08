namespace CustomCodeFramework.Notifications.SignalR.Options;

public sealed class SignalRNotificationOptions
{
    public const string SectionName = "Notifications:SignalR";

    public string HubPath { get; init; } = "/hubs/notifications";

    public string DefaultClientMethod { get; init; } = "ReceiveNotification";

    public string UserIdMetadataKey { get; init; } = "userId";

    public string GroupMetadataKey { get; init; } = "group";

    public bool EnableDetailedErrors { get; init; } = false;
}
