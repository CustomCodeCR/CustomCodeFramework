using CustomCodeFramework.Notifications.Channels;

namespace CustomCodeFramework.Notifications.Preferences;

public sealed record NotificationPreference
{
    public required NotificationPreferenceScope Scope { get; init; }

    public string? ScopeId { get; init; }

    public required NotificationChannelType Channel { get; init; }

    public required string NotificationType { get; init; }

    public bool IsEnabled { get; init; } = true;
}
