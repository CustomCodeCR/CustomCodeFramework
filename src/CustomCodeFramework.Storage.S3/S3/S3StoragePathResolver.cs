using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.S3.S3;

public sealed class S3StoragePathResolver(IOptions<S3StorageOptions> options)
{
    public string BucketName => options.Value.BucketName;

    public string ResolveKey(string storagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        var key = storagePath
            .Trim()
            .TrimStart('/', '\\')
            .Replace("\\", "/", StringComparison.Ordinal);

        if (key.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "S3 storage key cannot contain parent directory segments."
            );
        }

        return key;
    }

    public string ResolvePublicUrl(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
        {
            return $"{options.Value.PublicBaseUrl.TrimEnd('/')}/{key.TrimStart('/')}";
        }

        if (!string.IsNullOrWhiteSpace(options.Value.ServiceUrl))
        {
            return $"{options.Value.ServiceUrl.TrimEnd('/')}/{options.Value.BucketName}/{key.TrimStart('/')}";
        }

        return $"https://{options.Value.BucketName}.s3.{options.Value.Region}.amazonaws.com/{key.TrimStart('/')}";
    }
}
