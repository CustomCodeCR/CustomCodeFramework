namespace CustomCodeFramework.Notifications.Templates;

public sealed record NotificationTemplateContext
{
    public IReadOnlyDictionary<string, object?> Values { get; init; } =
        new Dictionary<string, object?>();

    public object? Model { get; init; }

    public string? Language { get; init; }
}
