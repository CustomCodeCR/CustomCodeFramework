using Amazon.S3;
using Amazon.S3.Model;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Security;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.S3.S3;

public sealed class S3FileStorage(
    IAmazonS3 s3Client,
    S3StoragePathResolver pathResolver,
    S3SignedUrlGenerator signedUrlGenerator,
    IContentTypeProvider contentTypeProvider,
    IOptions<S3StorageOptions> options
) : IFileStorageProvider
{
    public string ProviderName => "S3";

    public async Task<StoredFile> SaveAsync(
        FileObject file,
        FileVisibility visibility,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(file);

        var key = pathResolver.ResolveKey(file.Path);

        if (file.Content.Stream.CanSeek)
        {
            file.Content.Stream.Position = 0;
        }

        var contentType = string.IsNullOrWhiteSpace(file.Content.ContentType)
            ? contentTypeProvider.GetContentType(file.FileName)
            : file.Content.ContentType;

        var request = new PutObjectRequest
        {
            BucketName = pathResolver.BucketName,
            Key = key,
            InputStream = file.Content.Stream,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        foreach (var property in file.Metadata.Properties)
        {
            request.Metadata[property.Key] = property.Value;
        }

        if (!string.IsNullOrWhiteSpace(file.Metadata.OriginalFileName))
        {
            request.Metadata["original-file-name"] = file.Metadata.OriginalFileName;
        }

        if (!string.IsNullOrWhiteSpace(file.Metadata.UploadedBy))
        {
            request.Metadata["uploaded-by"] = file.Metadata.UploadedBy;
        }

        if (visibility == FileVisibility.Public)
        {
            request.CannedACL = S3CannedACL.PublicRead;
        }

        var response = await s3Client.PutObjectAsync(request, cancellationToken);

        var size = file.Content.Length ?? TryResolveLength(file.Content.Stream);

        return new StoredFile
        {
            FileId = key,
            FileName = file.FileName,
            Path = key,
            Url = BuildUrl(key, visibility),
            ContentType = contentType,
            SizeInBytes = size,
            Visibility = visibility,
            Metadata = file.Metadata with
            {
                ContentType = contentType,
                SizeInBytes = size,
                Checksum = string.IsNullOrWhiteSpace(response.ETag)
                    ? file.Metadata.Checksum
                    : new FileChecksum { Algorithm = "ETag", Value = response.ETag.Trim('"') },
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

        var key = pathResolver.ResolveKey(path);

        try
        {
            var response = await s3Client.GetObjectAsync(
                pathResolver.BucketName,
                key,
                cancellationToken
            );

            var memoryStream = new MemoryStream();

            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Position = 0;

            var fileName = Path.GetFileName(key);
            var contentType = string.IsNullOrWhiteSpace(response.Headers.ContentType)
                ? contentTypeProvider.GetContentType(fileName)
                : response.Headers.ContentType;

            return new FileObject
            {
                FileName = fileName,
                Path = key,
                Content = new FileContent
                {
                    Stream = memoryStream,
                    ContentType = contentType,
                    Length = memoryStream.Length,
                },
                Metadata = new FileMetadata
                {
                    OriginalFileName = TryGetMetadata(response, "original-file-name") ?? fileName,
                    ContentType = contentType,
                    SizeInBytes = memoryStream.Length,
                    UploadedBy = TryGetMetadata(response, "uploaded-by"),
                    CreatedAtUtc = response.LastModified?.ToUniversalTime() ?? DateTime.UtcNow,
                    Checksum = string.IsNullOrWhiteSpace(response.ETag)
                        ? null
                        : new FileChecksum { Algorithm = "ETag", Value = response.ETag.Trim('"') },
                },
            };
        }
        catch (AmazonS3Exception exception)
            when (exception.StatusCode == System.Net.HttpStatusCode.NotFound
                || exception.ErrorCode == "NoSuchKey"
            )
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var key = pathResolver.ResolveKey(path);

        try
        {
            await s3Client.GetObjectMetadataAsync(pathResolver.BucketName, key, cancellationToken);

            return true;
        }
        catch (AmazonS3Exception exception)
            when (exception.StatusCode == System.Net.HttpStatusCode.NotFound
                || exception.ErrorCode == "NotFound"
                || exception.ErrorCode == "NoSuchKey"
            )
        {
            return false;
        }
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var key = pathResolver.ResolveKey(path);

        return s3Client.DeleteObjectAsync(pathResolver.BucketName, key, cancellationToken);
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

    private string? BuildUrl(string key, FileVisibility visibility)
    {
        if (visibility == FileVisibility.Private)
        {
            return null;
        }

        if (!options.Value.UsePublicUrl)
        {
            return null;
        }

        return pathResolver.ResolvePublicUrl(key);
    }

    private static long TryResolveLength(Stream stream)
    {
        return stream.CanSeek ? stream.Length : 0;
    }

    private static string? TryGetMetadata(GetObjectResponse response, string key)
    {
        var metadataKey = key.StartsWith("x-amz-meta-", StringComparison.OrdinalIgnoreCase)
            ? key
            : $"x-amz-meta-{key}";

        return response.Metadata.Keys.Contains(metadataKey, StringComparer.OrdinalIgnoreCase)
            ? response.Metadata[metadataKey]
            : null;
    }
}
