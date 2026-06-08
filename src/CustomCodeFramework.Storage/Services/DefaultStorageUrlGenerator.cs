using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Options;
using CustomCodeFramework.Storage.Security;

namespace CustomCodeFramework.Storage.Services;

public sealed class DefaultStorageUrlGenerator(StorageOptions options) : IStorageUrlGenerator
{
    public string? GenerateUrl(StoredFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            return file.Url;
        }

        return $"{options.PublicBaseUrl.TrimEnd('/')}/{file.Path.TrimStart('/')}";
    }

    public Task<string?> GenerateSignedUrlAsync(
        string path,
        SignedUrlOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return Task.FromResult<string?>(null);
    }
}
