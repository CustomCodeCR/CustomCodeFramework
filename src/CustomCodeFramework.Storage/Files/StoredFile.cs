using CustomCodeFramework.Storage.Security;

namespace CustomCodeFramework.Storage.Files;

public sealed record StoredFile
{
    public required string FileId { get; init; }

    public required string FileName { get; init; }

    public required string Path { get; init; }

    public string? Url { get; init; }

    public required string ContentType { get; init; }

    public long SizeInBytes { get; init; }

    public FileVisibility Visibility { get; init; } = FileVisibility.Private;

    public FileMetadata Metadata { get; init; } = new();

    public DateTime StoredAtUtc { get; init; } = DateTime.UtcNow;
}
