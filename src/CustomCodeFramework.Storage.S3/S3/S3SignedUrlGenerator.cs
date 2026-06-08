using Amazon.S3;
using Amazon.S3.Model;
using CustomCodeFramework.Storage.Security;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.S3.S3;

public sealed class S3SignedUrlGenerator(
    IAmazonS3 s3Client,
    S3StoragePathResolver pathResolver,
    IOptions<S3StorageOptions> options
)
{
    public string GenerateSignedUrl(string path, SignedUrlOptions? signedUrlOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var key = pathResolver.ResolveKey(path);

        var expiresIn =
            signedUrlOptions?.ExpiresIn
            ?? TimeSpan.FromMinutes(options.Value.SignedUrlExpirationMinutes);

        var verb = ResolveVerb(signedUrlOptions?.Permission ?? FilePermission.Read);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = pathResolver.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiresIn),
            Verb = verb,
        };

        return s3Client.GetPreSignedURL(request);
    }

    private static HttpVerb ResolveVerb(FilePermission permission)
    {
        if (permission.HasFlag(FilePermission.Write))
        {
            return HttpVerb.PUT;
        }

        if (permission.HasFlag(FilePermission.Delete))
        {
            return HttpVerb.DELETE;
        }

        return HttpVerb.GET;
    }
}
