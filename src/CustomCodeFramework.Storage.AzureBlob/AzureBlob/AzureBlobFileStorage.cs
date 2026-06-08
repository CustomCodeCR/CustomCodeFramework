using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Security;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.AzureBlob.AzureBlob;

public sealed class AzureBlobFileStorage(
    BlobContainerClient containerClient,
    AzureBlobPathResolver pathResolver,
    AzureBlobSignedUrlGenerator signedUrlGenerator,
    IContentTypeProvider contentTypeProvider,
    IOptions<AzureBlobStorageOptions> options
) : IFileStorageProvider
{
    public string ProviderName => "AzureBlob";

    public async Task<StoredFile> SaveAsync(
        FileObject file,
        FileVisibility visibility,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(file);

        if (options.Value.CreateContainerIfNotExists)
        {
            await containerClient.CreateIfNotExistsAsync(
                ResolvePublicAccessType(visibility),
                cancellationToken: cancellationToken
            );
        }

        var blobName = pathResolver.ResolveBlobName(file.Path);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (file.Content.Stream.CanSeek)
        {
            file.Content.Stream.Position = 0;
        }

        var contentType = string.IsNullOrWhiteSpace(file.Content.ContentType)
            ? contentTypeProvider.GetContentType(file.FileName)
            : file.Content.ContentType;

        var metadata = BuildMetadata(file.Metadata);

        await blobClient.UploadAsync(
            file.Content.Stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Metadata = metadata,
                AccessTier = AccessTier.Hot,
            },
            cancellationToken
        );

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        var size = properties.Value.ContentLength;

        return new StoredFile
        {
            FileId = blobName,
            FileName = file.FileName,
            Path = blobName,
            Url = BuildUrl(blobClient, blobName, visibility),
            ContentType = contentType,
            SizeInBytes = size,
            Visibility = visibility,
            Metadata = file.Metadata with
            {
                ContentType = contentType,
                SizeInBytes = size,
                Checksum = properties.Value.ETag.ToString() is { Length: > 0 } etag
                    ? new FileChecksum { Algorithm = "ETag", Value = etag.Trim('"') }
                    : file.Metadata.Checksum,
            },
            StoredAtUtc = DateTime.UtcNow,
        };
    }

    public async Task<FileObject?> GetAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var blobName = pathResolver.ResolveBlobName(path);
        var blobClient = containerClient.GetBlobClient(blobName);

        try
        {
            var download = await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken
            );

            var memoryStream = new MemoryStream();

            await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Position = 0;

            var properties = await blobClient.GetPropertiesAsync(
                cancellationToken: cancellationToken
            );

            var fileName = Path.GetFileName(blobName);
            var contentType = string.IsNullOrWhiteSpace(properties.Value.ContentType)
                ? contentTypeProvider.GetContentType(fileName)
                : properties.Value.ContentType;

            return new FileObject
            {
                FileName = fileName,
                Path = blobName,
                Content = new FileContent
                {
                    Stream = memoryStream,
                    ContentType = contentType,
                    Length = memoryStream.Length,
                },
                Metadata = new FileMetadata
                {
                    OriginalFileName =
                        TryGetMetadata(properties.Value.Metadata, "originalFileName") ?? fileName,
                    ContentType = contentType,
                    SizeInBytes = memoryStream.Length,
                    UploadedBy = TryGetMetadata(properties.Value.Metadata, "uploadedBy"),
                    CreatedAtUtc = properties.Value.CreatedOn.UtcDateTime,
                    Checksum = properties.Value.ETag.ToString() is { Length: > 0 } etag
                        ? new FileChecksum { Algorithm = "ETag", Value = etag.Trim('"') }
                        : null,
                    Properties = properties.Value.Metadata.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase
                    ),
                },
            };
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var blobName = pathResolver.ResolveBlobName(path);
        var blobClient = containerClient.GetBlobClient(blobName);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var blobName = pathResolver.ResolveBlobName(path);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken
        );
    }

    public Task<string?> GenerateSignedUrlAsync(
        string path,
        SignedUrlOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(options);

        var url = signedUrlGenerator.GenerateSignedUrl(path, options);

        return Task.FromResult(url);
    }

    private string? BuildUrl(BlobClient blobClient, string blobName, FileVisibility visibility)
    {
        if (visibility == FileVisibility.Private)
        {
            return null;
        }

        if (!options.Value.UsePublicUrl)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
        {
            return pathResolver.ResolvePublicUrl(blobName);
        }

        return blobClient.Uri.ToString();
    }

    private static PublicAccessType ResolvePublicAccessType(FileVisibility visibility)
    {
        return visibility == FileVisibility.Public ? PublicAccessType.Blob : PublicAccessType.None;
    }

    private static Dictionary<string, string> BuildMetadata(FileMetadata metadata)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in metadata.Properties)
        {
            result[NormalizeMetadataKey(property.Key)] = property.Value;
        }

        if (!string.IsNullOrWhiteSpace(metadata.OriginalFileName))
        {
            result["originalFileName"] = metadata.OriginalFileName;
        }

        if (!string.IsNullOrWhiteSpace(metadata.UploadedBy))
        {
            result["uploadedBy"] = metadata.UploadedBy;
        }

        return result;
    }

    private static string NormalizeMetadataKey(string key)
    {
        var normalized = string.Concat(
            key.Select(character => char.IsLetterOrDigit(character) ? character : '_')
        );

        return string.IsNullOrWhiteSpace(normalized) ? "metadata" : normalized;
    }

    private static string? TryGetMetadata(IDictionary<string, string> metadata, string key)
    {
        return metadata.TryGetValue(key, out var value) ? value : null;
    }
}
