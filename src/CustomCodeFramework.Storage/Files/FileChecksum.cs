namespace CustomCodeFramework.Storage.Files;

public sealed record FileChecksum
{
    public required string Algorithm { get; init; }

    public required string Value { get; init; }
}
