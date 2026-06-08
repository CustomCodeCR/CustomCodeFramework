namespace CustomCodeFramework.Storage.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string ProviderName { get; init; } = "None";

    public string? DefaultBucket { get; init; }

    public string DefaultFolder { get; init; } = "files";

    public string? PublicBaseUrl { get; init; }

    public long MaxFileSizeInBytes { get; init; } = 25 * 1024 * 1024;

    public bool ValidateFileSignature { get; init; } = false;
}
