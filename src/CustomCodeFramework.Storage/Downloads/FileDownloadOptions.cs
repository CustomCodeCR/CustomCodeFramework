namespace CustomCodeFramework.Storage.Downloads;

public sealed record FileDownloadOptions
{
    public bool AsAttachment { get; init; }

    public string? DownloadFileName { get; init; }
}
