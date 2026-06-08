namespace CustomCodeFramework.Notifications.Channels;

public sealed record NotificationChannelResult
{
    public bool IsSuccess { get; init; }

    public bool IsFailure => !IsSuccess;

    public string? ProviderMessageId { get; init; }

    public NotificationChannelFailure? Failure { get; init; }

    public static NotificationChannelResult Success(string? providerMessageId = null)
    {
        return new NotificationChannelResult
        {
            IsSuccess = true,
            ProviderMessageId = providerMessageId,
        };
    }

    public static NotificationChannelResult Failed(
        string code,
        string message,
        Exception? exception = null
    )
    {
        return new NotificationChannelResult
        {
            IsSuccess = false,
            Failure = new NotificationChannelFailure
            {
                Code = code,
                Message = message,
                Exception = exception,
            },
        };
    }
}
