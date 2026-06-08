namespace CustomCodeFramework.Notifications.Messages;

public sealed record NotificationAttachment
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public string? Url { get; init; }

    public byte[]? Content { get; init; }

    public long? SizeInBytes { get; init; }
}
