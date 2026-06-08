using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Security;

namespace CustomCodeFramework.Storage.Abstractions;

public interface IFileStorageProvider
{
    string ProviderName { get; }

    Task<StoredFile> SaveAsync(
        FileObject file,
        FileVisibility visibility,
        CancellationToken cancellationToken = default
    );

    Task<FileObject?> GetAsync(string path, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    Task DeleteAsync(string path, CancellationToken cancellationToken = default);

    Task<string?> GenerateSignedUrlAsync(
        string path,
        SignedUrlOptions options,
        CancellationToken cancellationToken = default
    );
}
