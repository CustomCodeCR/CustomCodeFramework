using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Security;

namespace CustomCodeFramework.Storage.Abstractions;

public interface IStorageUrlGenerator
{
    string? GenerateUrl(StoredFile file);

    Task<string?> GenerateSignedUrlAsync(
        string path,
        SignedUrlOptions options,
        CancellationToken cancellationToken = default
    );
}
