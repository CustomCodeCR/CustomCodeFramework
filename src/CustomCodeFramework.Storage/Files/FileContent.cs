namespace CustomCodeFramework.Storage.Files;

public sealed record FileContent
{
    public required Stream Stream { get; init; }

    public required string ContentType { get; init; }

    public long? Length { get; init; }
}
