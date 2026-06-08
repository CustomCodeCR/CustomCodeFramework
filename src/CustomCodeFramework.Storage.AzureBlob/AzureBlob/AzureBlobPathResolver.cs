using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.AzureBlob.AzureBlob;

public sealed class AzureBlobPathResolver(IOptions<AzureBlobStorageOptions> options)
{
    public string ContainerName => options.Value.ContainerName;

    public string ResolveBlobName(string storagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        var blobName = storagePath
            .Trim()
            .TrimStart('/', '\\')
            .Replace("\\", "/", StringComparison.Ordinal);

        if (blobName.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Azure Blob name cannot contain parent directory segments."
            );
        }

        return blobName;
    }

    public string ResolvePublicUrl(string blobName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        if (!string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
        {
            return $"{options.Value.PublicBaseUrl.TrimEnd('/')}/{blobName.TrimStart('/')}";
        }

        return blobName;
    }
}
