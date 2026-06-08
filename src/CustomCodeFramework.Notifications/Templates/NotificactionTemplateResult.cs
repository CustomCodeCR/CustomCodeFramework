namespace CustomCodeFramework.Notifications.Templates;

public sealed record NotificationTemplateResult
{
    public string? Subject { get; init; }

    public required string Body { get; init; }

    public bool IsHtml { get; init; }
}
