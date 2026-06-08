using CustomCodeFramework.Notifications.Templates;

namespace CustomCodeFramework.Notifications.Abstractions;

public interface INotificationTemplateRenderer
{
    Task<NotificationTemplateResult> RenderAsync(
        NotificationTemplate template,
        NotificationTemplateContext context,
        CancellationToken cancellationToken = default
    );
}
