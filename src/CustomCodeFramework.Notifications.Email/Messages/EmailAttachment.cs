namespace CustomCodeFramework.Notifications.Email.Messages;

public sealed record EmailAttachment
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }
}
