namespace CustomCodeFramework.Notifications.Channels;

public sealed record NotificationChannelFailure
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public Exception? Exception { get; init; }
}
