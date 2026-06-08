namespace CustomCodeFramework.Messaging.Inbox;

public sealed record InboxOptions
{
    public const string SectionName = "Messaging:Inbox";

    public bool Enabled { get; init; } = true;

    public int ProcessedMessageExpirationDays { get; init; } = 30;
}
