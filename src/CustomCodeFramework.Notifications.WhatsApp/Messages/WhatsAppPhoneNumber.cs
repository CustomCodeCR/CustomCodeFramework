namespace CustomCodeFramework.Notifications.WhatsApp.Messages;

public sealed record WhatsAppPhoneNumber
{
    public required string Value { get; init; }

    public string? CountryCode { get; init; }

    public string ToE164(string defaultCountryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultCountryCode);

        var normalized = Value
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty);

        if (normalized.StartsWith('+'))
        {
            return normalized;
        }

        var countryCode = string.IsNullOrWhiteSpace(CountryCode) ? defaultCountryCode : CountryCode;

        countryCode = countryCode.Trim().TrimStart('+');

        return $"+{countryCode}{normalized}";
    }
}
