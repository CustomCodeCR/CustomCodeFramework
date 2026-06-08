namespace CustomCodeFramework.Notifications.Email.Messages;

public sealed record EmailAddress
{
    public required string Address { get; init; }

    public string? Name { get; init; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Name) ? Address : $"{Name} <{Address}>";
    }
}
