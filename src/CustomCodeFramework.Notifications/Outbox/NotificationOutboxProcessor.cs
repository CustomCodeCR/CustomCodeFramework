using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Notifications.Outbox;

public sealed class NotificationOutboxProcessor(
    INotificationOutboxStore outboxStore,
    INotificationDispatcher dispatcher,
    IOptions<NotificationsOptions> options,
    ILogger<NotificationOutboxProcessor> logger
)
{
    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var pendingMessages = await outboxStore.GetPendingAsync(
            options.Value.OutboxBatchSize,
            now,
            cancellationToken
        );

        foreach (var outboxMessage in pendingMessages)
        {
            try
            {
                var result = await dispatcher.DispatchAsync(
                    outboxMessage.Message,
                    cancellationToken
                );

                if (result.IsSuccess)
                {
                    await outboxStore.MarkAsProcessedAsync(
                        outboxMessage.Id,
                        DateTime.UtcNow,
                        cancellationToken
                    );

                    continue;
                }

                await MarkAsFailedAsync(
                    outboxMessage,
                    result.ErrorMessage ?? "Notification delivery failed.",
                    cancellationToken
                );
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to process notification outbox message {NotificationOutboxMessageId}",
                    outboxMessage.Id
                );

                await MarkAsFailedAsync(outboxMessage, exception.Message, cancellationToken);
            }
        }
    }

    private Task MarkAsFailedAsync(
        NotificationOutboxMessage outboxMessage,
        string error,
        CancellationToken cancellationToken
    )
    {
        var nextAttemptAtUtc = DateTime.UtcNow.AddSeconds(options.Value.OutboxRetryDelaySeconds);

        return outboxStore.MarkAsFailedAsync(
            outboxMessage.Id,
            error,
            nextAttemptAtUtc,
            cancellationToken
        );
    }
}
