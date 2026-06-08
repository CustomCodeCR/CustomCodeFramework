namespace CustomCodeFramework.Storage.Uploads;

public sealed record FileUploadRequest
{
    public required string FileName { get; init; }

    public required Stream Content { get; init; }

    public string? ContentType { get; init; }

    public long? SizeInBytes { get; init; }

    public string? UploadedBy { get; init; }

    public FileUploadOptions Options { get; init; } = new();
}
