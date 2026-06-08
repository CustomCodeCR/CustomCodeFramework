namespace CustomCodeFramework.Storage.Files;

public sealed record FileMetadata
{
    public string? OriginalFileName { get; init; }

    public string? ContentType { get; init; }

    public long SizeInBytes { get; init; }

    public string? UploadedBy { get; init; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public FileChecksum? Checksum { get; init; }

    public IReadOnlyDictionary<string, string> Properties { get; init; } =
        new Dictionary<string, string>();
}
