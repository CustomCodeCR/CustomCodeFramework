using CustomCodeFramework.Notifications.Channels;

namespace CustomCodeFramework.Notifications.Templates;

public sealed record NotificationTemplate
{
    public required string Key { get; init; }

    public required NotificationChannelType Channel { get; init; }

    public string? SubjectTemplate { get; init; }

    public required string BodyTemplate { get; init; }

    public string? Language { get; init; }

    public bool IsHtml { get; init; }
}
