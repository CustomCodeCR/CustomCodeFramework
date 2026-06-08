using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Templates;

namespace CustomCodeFramework.Notifications.Email.Templates;

public sealed class EmailTemplateRenderer : INotificationTemplateRenderer
{
    public Task<NotificationTemplateResult> RenderAsync(
        NotificationTemplate template,
        NotificationTemplateContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(context);

        if (template.Channel != NotificationChannelType.Email)
        {
            throw new InvalidOperationException(
                "The notification template is not an email template."
            );
        }

        var subject = RenderText(template.SubjectTemplate, context);
        var body = RenderText(template.BodyTemplate, context);

        var result = new NotificationTemplateResult
        {
            Subject = subject,
            Body = body,
            IsHtml = template.IsHtml,
        };

        return Task.FromResult(result);
    }

    private static string? RenderText(string? text, NotificationTemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var rendered = text;

        foreach (var value in context.Values)
        {
            rendered = rendered.Replace(
                "{{" + value.Key + "}}",
                value.Value?.ToString(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        return rendered;
    }
}
