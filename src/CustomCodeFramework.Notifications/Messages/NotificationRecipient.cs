namespace CustomCodeFramework.Notifications.Messages;

public sealed record NotificationRecipient
{
    public string? UserId { get; init; }

    public string? Name { get; init; }

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? DeviceToken { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
