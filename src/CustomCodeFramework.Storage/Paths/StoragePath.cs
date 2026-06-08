namespace CustomCodeFramework.Storage.Paths;

public sealed record StoragePath
{
    public string? Bucket { get; init; }

    public string? Folder { get; init; }

    public required string FileName { get; init; }

    public string FullPath
    {
        get
        {
            var parts = new[] { Bucket, Folder, FileName }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part!.Trim('/'));

            return string.Join("/", parts);
        }
    }
}
