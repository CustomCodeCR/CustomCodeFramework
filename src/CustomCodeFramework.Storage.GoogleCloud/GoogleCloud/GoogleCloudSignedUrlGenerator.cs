using CustomCodeFramework.Storage.Security;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.GoogleCloud.GoogleCloud;

public sealed class GoogleCloudSignedUrlGenerator(
    UrlSigner urlSigner,
    GoogleCloudPathResolver pathResolver,
    IOptions<GoogleCloudStorageOptions> options
)
{
    public string GenerateSignedUrl(string path, SignedUrlOptions? signedUrlOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var objectName = pathResolver.ResolveObjectName(path);

        var expiresIn =
            signedUrlOptions?.ExpiresIn
            ?? TimeSpan.FromMinutes(options.Value.SignedUrlExpirationMinutes);

        var httpMethod = ResolveHttpMethod(signedUrlOptions?.Permission ?? FilePermission.Read);

        return urlSigner.Sign(pathResolver.BucketName, objectName, expiresIn, httpMethod);
    }

    private static HttpMethod ResolveHttpMethod(FilePermission permission)
    {
        if (permission.HasFlag(FilePermission.Write))
        {
            return HttpMethod.Put;
        }

        if (permission.HasFlag(FilePermission.Delete))
        {
            return HttpMethod.Delete;
        }

        return HttpMethod.Get;
    }
}
