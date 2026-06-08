using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

namespace CustomCodeFramework.Notifications.Delivery;

public sealed record NotificationDelivery
{
    public Guid DeliveryId { get; init; } = Guid.NewGuid();

    public Guid MessageId { get; init; }

    public NotificationChannelType Channel { get; init; }

    public NotificationRecipient Recipient { get; init; } = default!;

    public NotificationDeliveryStatus Status { get; init; } = NotificationDeliveryStatus.Pending;

    public int AttemptCount { get; init; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? SentAtUtc { get; init; }

    public DateTime? FailedAtUtc { get; init; }

    public IReadOnlyCollection<NotificationDeliveryAttempt> Attempts { get; init; } = [];
}
