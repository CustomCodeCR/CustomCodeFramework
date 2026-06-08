namespace CustomCodeFramework.Storage.Downloads;

public sealed record FileDownloadRequest
{
    public required string Path { get; init; }

    public FileDownloadOptions Options { get; init; } = new();
}
