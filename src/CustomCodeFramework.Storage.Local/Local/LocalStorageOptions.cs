namespace CustomCodeFramework.Storage.Local.Local;

public sealed class LocalStorageOptions
{
    public const string SectionName = "Storage:Local";

    public string RootPath { get; init; } = "storage";

    public bool CreateDirectoryIfNotExists { get; init; } = true;

    public bool UsePublicUrl { get; init; } = false;

    public string? PublicBaseUrl { get; init; }
}
