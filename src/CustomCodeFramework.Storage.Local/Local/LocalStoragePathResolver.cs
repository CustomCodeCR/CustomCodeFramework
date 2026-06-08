using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.Local.Local;

public sealed class LocalStoragePathResolver(IOptions<LocalStorageOptions> options)
{
    public string ResolvePhysicalPath(string storagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        var safeStoragePath = NormalizeStoragePath(storagePath);
        var rootPath = Path.GetFullPath(options.Value.RootPath);
        var fullPath = Path.GetFullPath(Path.Combine(rootPath, safeStoragePath));

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The resolved file path is outside the configured storage root."
            );
        }

        return fullPath;
    }

    public string ResolveRelativePath(string physicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);

        var rootPath = Path.GetFullPath(options.Value.RootPath);
        var fullPath = Path.GetFullPath(physicalPath);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The physical file path is outside the configured storage root."
            );
        }

        return Path.GetRelativePath(rootPath, fullPath)
            .Replace("\\", "/", StringComparison.Ordinal);
    }

    public string ResolveDirectory(string storagePath)
    {
        var physicalPath = ResolvePhysicalPath(storagePath);

        return Path.GetDirectoryName(physicalPath) ?? Path.GetFullPath(options.Value.RootPath);
    }

    private static string NormalizeStoragePath(string storagePath)
    {
        var normalized = storagePath
            .Trim()
            .TrimStart('/', '\\')
            .Replace("\\", "/", StringComparison.Ordinal);

        if (normalized.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Storage path cannot contain parent directory segments."
            );
        }

        return normalized;
    }
}
