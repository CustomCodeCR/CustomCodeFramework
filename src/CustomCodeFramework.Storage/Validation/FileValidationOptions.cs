namespace CustomCodeFramework.Storage.Validation;

public sealed record FileValidationOptions
{
    public long MaxSizeInBytes { get; init; } = 25 * 1024 * 1024;

    public IReadOnlyCollection<string> AllowedExtensions { get; init; } =
    [".pdf", ".csv", ".xlsx", ".docx", ".png", ".jpg", ".jpeg", ".webp"];

    public IReadOnlyCollection<string> AllowedContentTypes { get; init; } = [];

    public bool ValidateFileSignature { get; init; } = false;
}
