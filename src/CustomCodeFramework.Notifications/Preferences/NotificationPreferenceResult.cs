namespace CustomCodeFramework.Notifications.Preferences;

public sealed record NotificationPreferenceResult
{
    public bool CanSend { get; init; }

    public string? Reason { get; init; }

    public static NotificationPreferenceResult Allowed()
    {
        return new NotificationPreferenceResult { CanSend = true };
    }

    public static NotificationPreferenceResult Blocked(string reason)
    {
        return new NotificationPreferenceResult { CanSend = false, Reason = reason };
    }
}
