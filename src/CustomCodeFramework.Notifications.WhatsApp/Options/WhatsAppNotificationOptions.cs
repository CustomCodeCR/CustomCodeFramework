namespace CustomCodeFramework.Notifications.WhatsApp.Options;

public sealed class WhatsAppNotificationOptions
{
    public const string SectionName = "Notifications:WhatsApp";

    public string DefaultCountryCode { get; init; } = "506";

    public string ProviderName { get; init; } = "None";

    public bool Enabled { get; init; } = false;

    public string? DefaultLanguageCode { get; init; } = "es";
}
