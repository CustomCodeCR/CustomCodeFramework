namespace CustomCodeFramework.Storage.Firebase.Firebase;

public sealed class FirebaseStorageOptions
{
    public const string SectionName = "Storage:Firebase";

    public string BucketName { get; init; } = string.Empty;

    public string? ProjectId { get; init; }

    public string? CredentialFilePath { get; init; }

    public bool UsePublicUrl { get; init; } = false;

    public string? PublicBaseUrl { get; init; }

    public int SignedUrlExpirationMinutes { get; init; } = 15;

    public bool UseFirebaseDownloadToken { get; init; } = true;
}
