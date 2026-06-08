using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Security;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.Local.Local;

public sealed class LocalFileStorage(
    LocalStoragePathResolver pathResolver,
    IContentTypeProvider contentTypeProvider,
    IOptions<LocalStorageOptions> options
) : IFileStorageProvider
{
    public string ProviderName => "Local";

    public async Task<StoredFile> SaveAsync(
        FileObject file,
        FileVisibility visibility,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(file);

        var physicalPath = pathResolver.ResolvePhysicalPath(file.Path);
        var directory = Path.GetDirectoryName(physicalPath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("Local storage directory could not be resolved.");
        }

        if (!Directory.Exists(directory))
        {
            if (!options.Value.CreateDirectoryIfNotExists)
            {
                throw new DirectoryNotFoundException(directory);
            }

            Directory.CreateDirectory(directory);
        }

        await using (
            var output = new FileStream(
                physicalPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true
            )
        )
        {
            if (file.Content.Stream.CanSeek)
            {
                file.Content.Stream.Position = 0;
            }

            await file.Content.Stream.CopyToAsync(output, cancellationToken);
        }

        var fileInfo = new FileInfo(physicalPath);
        var relativePath = pathResolver.ResolveRelativePath(physicalPath);
        var contentType = string.IsNullOrWhiteSpace(file.Content.ContentType)
            ? contentTypeProvider.GetContentType(file.FileName)
            : file.Content.ContentType;

        return new StoredFile
        {
            FileId = relativePath,
            FileName = file.FileName,
            Path = relativePath,
            Url = BuildPublicUrl(relativePath, visibility),
            ContentType = contentType,
            SizeInBytes = fileInfo.Length,
            Visibility = visibility,
            Metadata = file.Metadata with
            {
                ContentType = contentType,
                SizeInBytes = fileInfo.Length,
            },
            StoredAtUtc = DateTime.UtcNow,
        };
    }

    public Task<FileObject?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var physicalPath = pathResolver.ResolvePhysicalPath(path);

        if (!File.Exists(physicalPath))
        {
            return Task.FromResult<FileObject?>(null);
        }

        var fileName = Path.GetFileName(physicalPath);
        var contentType = contentTypeProvider.GetContentType(fileName);
        var fileInfo = new FileInfo(physicalPath);

        Stream stream = new FileStream(
            physicalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true
        );

        var fileObject = new FileObject
        {
            FileName = fileName,
            Path = pathResolver.ResolveRelativePath(physicalPath),
            Content = new FileContent
            {
                Stream = stream,
                ContentType = contentType,
                Length = fileInfo.Length,
            },
            Metadata = new FileMetadata
            {
                OriginalFileName = fileName,
                ContentType = contentType,
                SizeInBytes = fileInfo.Length,
                CreatedAtUtc = fileInfo.CreationTimeUtc,
            },
        };

        return Task.FromResult<FileObject?>(fileObject);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var physicalPath = pathResolver.ResolvePhysicalPath(path);

        return Task.FromResult(File.Exists(physicalPath));
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var physicalPath = pathResolver.ResolvePhysicalPath(path);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    public Task<string?> GenerateSignedUrlAsync(
        string path,
        SignedUrlOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(options);

        return Task.FromResult(BuildPublicUrl(path, FileVisibility.Protected));
    }

    private string? BuildPublicUrl(string path, FileVisibility visibility)
    {
        if (!options.Value.UsePublicUrl)
        {
            return null;
        }

        if (visibility == FileVisibility.Private)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
        {
            return null;
        }

        return $"{options.Value.PublicBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
