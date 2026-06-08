namespace CustomCodeFramework.Notifications.WhatsApp.Messages;

public sealed record WhatsAppTemplateMessage
{
    public required string TemplateName { get; init; }

    public string LanguageCode { get; init; } = "es";

    public IReadOnlyCollection<string> BodyParameters { get; init; } = [];

    public IReadOnlyCollection<string> HeaderParameters { get; init; } = [];

    public IReadOnlyCollection<string> ButtonParameters { get; init; } = [];
}
