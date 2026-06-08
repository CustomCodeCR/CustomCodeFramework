using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.GoogleCloud.GoogleCloud;

public sealed class GoogleCloudPathResolver(IOptions<GoogleCloudStorageOptions> options)
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
                "Google Cloud object name cannot contain parent directory segments."
            );
        }

        return objectName;
    }

    public string ResolvePublicUrl(string objectName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectName);

        if (!string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
        {
            return $"{options.Value.PublicBaseUrl.TrimEnd('/')}/{objectName.TrimStart('/')}";
        }

        return $"https://storage.googleapis.com/{options.Value.BucketName}/{objectName.TrimStart('/')}";
    }
}
