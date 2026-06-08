namespace CustomCodeFramework.Storage.AzureBlob.AzureBlob;

public sealed class AzureBlobStorageOptions
{
    public const string SectionName = "Storage:AzureBlob";

    public string ConnectionString { get; init; } = string.Empty;

    public string ContainerName { get; init; } = string.Empty;

    public bool CreateContainerIfNotExists { get; init; } = true;

    public bool UsePublicUrl { get; init; } = false;

    public string? PublicBaseUrl { get; init; }

    public int SignedUrlExpirationMinutes { get; init; } = 15;
}
