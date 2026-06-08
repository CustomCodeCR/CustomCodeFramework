using CustomCodeFramework.Notifications.Channels;

namespace CustomCodeFramework.Notifications.Delivery;

public sealed record NotificationDeliveryResult
{
    public Guid MessageId { get; init; }

    public NotificationChannelType Channel { get; init; }

    public NotificationDeliveryStatus Status { get; init; }

    public string? ProviderMessageId { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public bool IsSuccess => Status == NotificationDeliveryStatus.Sent;

    public static NotificationDeliveryResult Sent(
        Guid messageId,
        NotificationChannelType channel,
        string? providerMessageId = null
    )
    {
        return new NotificationDeliveryResult
        {
            MessageId = messageId,
            Channel = channel,
            Status = NotificationDeliveryStatus.Sent,
            ProviderMessageId = providerMessageId,
        };
    }

    public static NotificationDeliveryResult Failed(
        Guid messageId,
        NotificationChannelType channel,
        string errorCode,
        string errorMessage
    )
    {
        return new NotificationDeliveryResult
        {
            MessageId = messageId,
            Channel = channel,
            Status = NotificationDeliveryStatus.Failed,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }
}
