namespace CustomCodeFramework.Notifications.Options;

public sealed class NotificationsOptions
{
    public const string SectionName = "Notifications";

    public bool UseOutbox { get; init; } = false;

    public int OutboxBatchSize { get; init; } = 50;

    public int OutboxMaxRetryCount { get; init; } = 5;

    public int OutboxRetryDelaySeconds { get; init; } = 30;
}
