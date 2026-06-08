namespace CustomCodeFramework.Notifications.Delivery;

public sealed record NotificationDeliveryAttempt
{
    public required int AttemptNumber { get; init; }

    public required DateTime AttemptedAtUtc { get; init; }

    public bool IsSuccess { get; init; }

    public string? ProviderMessageId { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }
}
