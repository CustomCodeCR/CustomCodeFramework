using CustomCodeFramework.Storage.Security;

namespace CustomCodeFramework.Storage.Uploads;

public sealed record FileUploadOptions
{
    public string? Folder { get; init; }

    public FileVisibility Visibility { get; init; } = FileVisibility.Private;

    public bool GenerateUniqueFileName { get; init; } = true;

    public IReadOnlyCollection<string> AllowedExtensions { get; init; } = [];

    public IReadOnlyCollection<string> AllowedContentTypes { get; init; } = [];

    public long? MaxSizeInBytes { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
