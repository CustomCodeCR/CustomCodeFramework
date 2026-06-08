namespace CustomCodeFramework.Storage.GoogleCloud.GoogleCloud;

public sealed class GoogleCloudStorageOptions
{
    public const string SectionName = "Storage:GoogleCloud";

    public string BucketName { get; init; } = string.Empty;

    public string? ProjectId { get; init; }

    public string? CredentialFilePath { get; init; }

    public bool CreateBucketIfNotExists { get; init; } = false;

    public bool UsePublicUrl { get; init; } = false;

    public string? PublicBaseUrl { get; init; }

    public int SignedUrlExpirationMinutes { get; init; } = 15;
}
