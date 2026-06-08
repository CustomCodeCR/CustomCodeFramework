using CustomCodeFramework.Notifications.Messages;
using CustomCodeFramework.Notifications.WhatsApp.Options;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.WhatsApp.Messages;

public sealed class WhatsAppNotificationMessageMapper(IOptions<WhatsAppNotificationOptions> options)
{
    public WhatsAppNotificationMessage Map(NotificationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.Recipient.PhoneNumber))
        {
            throw new InvalidOperationException("WhatsApp recipient phone number is required.");
        }

        var template = TryMapTemplate(message);

        return new WhatsAppNotificationMessage
        {
            MessageId = message.MessageId,
            To = new WhatsAppPhoneNumber
            {
                Value = message.Recipient.PhoneNumber,
                CountryCode = options.Value.DefaultCountryCode,
            },
            Body = template is null ? message.Body : null,
            Template = template,
            Data = message.Data,
            CreatedAtUtc = message.CreatedAtUtc,
        };
    }

    private WhatsAppTemplateMessage? TryMapTemplate(NotificationMessage message)
    {
        if (
            !message.Data.TryGetValue("templateName", out var templateNameValue)
            || templateNameValue is null
            || string.IsNullOrWhiteSpace(templateNameValue.ToString())
        )
        {
            return null;
        }

        var languageCode = ResolveString(
            message,
            "languageCode",
            options.Value.DefaultLanguageCode ?? "es"
        );

        return new WhatsAppTemplateMessage
        {
            TemplateName = templateNameValue.ToString()!,
            LanguageCode = languageCode,
            BodyParameters = ResolveStringCollection(message, "bodyParameters"),
            HeaderParameters = ResolveStringCollection(message, "headerParameters"),
            ButtonParameters = ResolveStringCollection(message, "buttonParameters"),
        };
    }

    private static string ResolveString(
        NotificationMessage message,
        string key,
        string defaultValue
    )
    {
        if (!message.Data.TryGetValue(key, out var value) || value is null)
        {
            return defaultValue;
        }

        return string.IsNullOrWhiteSpace(value.ToString()) ? defaultValue : value.ToString()!;
    }

    private static IReadOnlyCollection<string> ResolveStringCollection(
        NotificationMessage message,
        string key
    )
    {
        if (!message.Data.TryGetValue(key, out var value) || value is null)
        {
            return [];
        }

        if (value is IReadOnlyCollection<string> readOnlyCollection)
        {
            return readOnlyCollection;
        }

        if (value is IEnumerable<string> enumerable)
        {
            return enumerable.ToArray();
        }

        return [value.ToString() ?? string.Empty];
    }
}
