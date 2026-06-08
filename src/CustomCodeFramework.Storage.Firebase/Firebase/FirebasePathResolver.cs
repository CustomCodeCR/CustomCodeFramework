using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.Firebase.Firebase;

public sealed class FirebasePathResolver(IOptions<FirebaseStorageOptions> options)
{
    public string BucketName => options.Value.BucketName;

    public string ResolveObjectName(string storagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        var objectName = storagePath
            .Trim()
            .TrimStart('/', '\\')
            .Replace("\\", "/", StringComparison.Ordinal);

        if (objectName.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Firebase object name cannot contain parent directory segments."
            );
        }

        return objectName;
    }

    public string ResolvePublicUrl(string objectName, string? firebaseDownloadToken = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectName);

        if (!string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
        {
            return $"{options.Value.PublicBaseUrl.TrimEnd('/')}/{objectName.TrimStart('/')}";
        }

        var encodedObjectName = Uri.EscapeDataString(objectName);

        var url =
            $"https://firebasestorage.googleapis.com/v0/b/{options.Value.BucketName}/o/{encodedObjectName}?alt=media";

        if (!string.IsNullOrWhiteSpace(firebaseDownloadToken))
        {
            url += $"&token={Uri.EscapeDataString(firebaseDownloadToken)}";
        }

        return url;
    }
}
