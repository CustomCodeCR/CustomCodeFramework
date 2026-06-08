namespace CustomCodeFramework.Storage.S3.S3;

public sealed class S3StorageOptions
{
    public const string SectionName = "Storage:S3";

    public string BucketName { get; init; } = string.Empty;

    public string Region { get; init; } = "us-east-1";

    public string? AccessKey { get; init; }

    public string? SecretKey { get; init; }

    public string? ServiceUrl { get; init; }

    public bool ForcePathStyle { get; init; } = false;

    public bool UsePublicUrl { get; init; } = false;

    public string? PublicBaseUrl { get; init; }

    public int SignedUrlExpirationMinutes { get; init; } = 15;
}
