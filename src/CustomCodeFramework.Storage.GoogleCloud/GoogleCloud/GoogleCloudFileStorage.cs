using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Security;
using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.GoogleCloud.GoogleCloud;

public sealed class GoogleCloudFileStorage(
    StorageClient storageClient,
    GoogleCloudPathResolver pathResolver,
    GoogleCloudSignedUrlGenerator signedUrlGenerator,
    IContentTypeProvider contentTypeProvider,
    IOptions<GoogleCloudStorageOptions> options
) : IFileStorageProvider
{
    public string ProviderName => "GoogleCloud";

    public async Task<StoredFile> SaveAsync(
        FileObject file,
        FileVisibility visibility,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(file);

        if (options.Value.CreateBucketIfNotExists)
        {
            await CreateBucketIfNotExistsAsync(cancellationToken);
        }

        var objectName = pathResolver.ResolveObjectName(file.Path);

        if (file.Content.Stream.CanSeek)
        {
            file.Content.Stream.Position = 0;
        }

        var contentType = string.IsNullOrWhiteSpace(file.Content.ContentType)
            ? contentTypeProvider.GetContentType(file.FileName)
            : file.Content.ContentType;

        var metadata = BuildMetadata(file.Metadata);

        var storageObject = new Google.Apis.Storage.v1.Data.Object
        {
            Bucket = pathResolver.BucketName,
            Name = objectName,
            ContentType = contentType,
            Metadata = metadata,
        };

        var uploadedObject = await storageClient.UploadObjectAsync(
            storageObject,
            file.Content.Stream,
            cancellationToken: cancellationToken
        );

        if (visibility == FileVisibility.Public)
        {
            uploadedObject.Acl = [new ObjectAccessControl { Entity = "allUsers", Role = "READER" }];

            await storageClient.UpdateObjectAsync(
                uploadedObject,
                new UpdateObjectOptions { Projection = Projection.Full },
                cancellationToken: cancellationToken
            );
        }

        var size = uploadedObject.Size is null
            ? file.Content.Length ?? TryResolveLength(file.Content.Stream)
            : Convert.ToInt64(uploadedObject.Size.Value);

        return new StoredFile
        {
            FileId = objectName,
            FileName = file.FileName,
            Path = objectName,
            Url = BuildUrl(objectName, visibility),
            ContentType = contentType,
            SizeInBytes = size,
            Visibility = visibility,
            Metadata = file.Metadata with
            {
                ContentType = contentType,
                SizeInBytes = size,
                Checksum = string.IsNullOrWhiteSpace(uploadedObject.Md5Hash)
                    ? file.Metadata.Checksum
                    : new FileChecksum { Algorithm = "MD5", Value = uploadedObject.Md5Hash },
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

        var objectName = pathResolver.ResolveObjectName(path);

        try
        {
            var storageObject = await storageClient.GetObjectAsync(
                pathResolver.BucketName,
                objectName,
                cancellationToken: cancellationToken
            );

            var memoryStream = new MemoryStream();

            await storageClient.DownloadObjectAsync(
                pathResolver.BucketName,
                objectName,
                memoryStream,
                cancellationToken: cancellationToken
            );

            memoryStream.Position = 0;

            var fileName = Path.GetFileName(objectName);
            var contentType = string.IsNullOrWhiteSpace(storageObject.ContentType)
                ? contentTypeProvider.GetContentType(fileName)
                : storageObject.ContentType;

            return new FileObject
            {
                FileName = fileName,
                Path = objectName,
                Content = new FileContent
                {
                    Stream = memoryStream,
                    ContentType = contentType,
                    Length = memoryStream.Length,
                },
                Metadata = new FileMetadata
                {
                    OriginalFileName =
                        TryGetMetadata(storageObject.Metadata, "originalFileName") ?? fileName,
                    ContentType = contentType,
                    SizeInBytes = memoryStream.Length,
                    UploadedBy = TryGetMetadata(storageObject.Metadata, "uploadedBy"),
                    CreatedAtUtc =
                        storageObject.TimeCreatedDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
                    Checksum = string.IsNullOrWhiteSpace(storageObject.Md5Hash)
                        ? null
                        : new FileChecksum { Algorithm = "MD5", Value = storageObject.Md5Hash },
                    Properties =
                        storageObject.Metadata?.ToDictionary(
                            pair => pair.Key,
                            pair => pair.Value,
                            StringComparer.OrdinalIgnoreCase
                        ) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                },
            };
        }
        catch (GoogleApiException exception) when (exception.Error?.Code == 404)
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var objectName = pathResolver.ResolveObjectName(path);

        try
        {
            await storageClient.GetObjectAsync(
                pathResolver.BucketName,
                objectName,
                cancellationToken: cancellationToken
            );

            return true;
        }
        catch (GoogleApiException exception) when (exception.Error?.Code == 404)
        {
            return false;
        }
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var objectName = pathResolver.ResolveObjectName(path);

        return storageClient.DeleteObjectAsync(
            pathResolver.BucketName,
            objectName,
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

        return Task.FromResult<string?>(url);
    }

    private async Task CreateBucketIfNotExistsAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ProjectId))
        {
            return;
        }

        try
        {
            await storageClient.GetBucketAsync(
                pathResolver.BucketName,
                cancellationToken: cancellationToken
            );
        }
        catch (GoogleApiException exception) when (exception.Error?.Code == 404)
        {
            await storageClient.CreateBucketAsync(
                options.Value.ProjectId,
                pathResolver.BucketName,
                cancellationToken: cancellationToken
            );
        }
    }

    private string? BuildUrl(string objectName, FileVisibility visibility)
    {
        if (visibility == FileVisibility.Private)
        {
            return null;
        }

        if (!options.Value.UsePublicUrl)
        {
            return null;
        }

        return pathResolver.ResolvePublicUrl(objectName);
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
            key.Select(character =>
                char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_'
            )
        );

        return string.IsNullOrWhiteSpace(normalized) ? "metadata" : normalized;
    }

    private static string? TryGetMetadata(IDictionary<string, string>? metadata, string key)
    {
        if (metadata is null)
        {
            return null;
        }

        return metadata.TryGetValue(key, out var value) ? value : null;
    }

    private static long TryResolveLength(Stream stream)
    {
        return stream.CanSeek ? stream.Length : 0;
    }
}
