namespace CustomCodeFramework.Storage.Files;

public sealed record FileObject
{
    public required string FileName { get; init; }

    public required string Path { get; init; }

    public required FileContent Content { get; init; }

    public FileMetadata Metadata { get; init; } = new();
}
