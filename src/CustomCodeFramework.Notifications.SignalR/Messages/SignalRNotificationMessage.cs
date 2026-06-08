namespace CustomCodeFramework.Notifications.SignalR.Messages;

public sealed record SignalRNotificationMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public required SignalRTarget Target { get; init; }

    public required string ClientMethod { get; init; }

    public string? Subject { get; init; }

    public required string Body { get; init; }

    public object? Payload { get; init; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
