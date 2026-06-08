namespace CustomCodeFramework.Reports.Storage;

public sealed record ReportStoredFile
{
    public required string FileName { get; init; }

    public required string Path { get; init; }

    public string? Url { get; init; }

    public required string ContentType { get; init; }

    public long SizeInBytes { get; init; }

    public DateTime StoredAtUtc { get; init; } = DateTime.UtcNow;
}
