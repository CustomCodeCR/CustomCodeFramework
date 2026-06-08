namespace CustomCodeFramework.Notifications.Sms.Options;

public sealed class SmsNotificationOptions
{
    public const string SectionName = "Notifications:Sms";

    public string DefaultCountryCode { get; init; } = "506";

    public string ProviderName { get; init; } = "None";

    public bool Enabled { get; init; } = false;
}
