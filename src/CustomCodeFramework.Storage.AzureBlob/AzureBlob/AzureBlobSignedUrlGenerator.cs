using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CustomCodeFramework.Storage.Security;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Storage.AzureBlob.AzureBlob;

public sealed class AzureBlobSignedUrlGenerator(
    BlobContainerClient containerClient,
    AzureBlobPathResolver pathResolver,
    IOptions<AzureBlobStorageOptions> options
)
{
    public string? GenerateSignedUrl(string path, SignedUrlOptions? signedUrlOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var blobName = pathResolver.ResolveBlobName(path);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            return null;
        }

        var expiresIn =
            signedUrlOptions?.ExpiresIn
            ?? TimeSpan.FromMinutes(options.Value.SignedUrlExpirationMinutes);

        var permissions = ResolvePermissions(signedUrlOptions?.Permission ?? FilePermission.Read);

        var builder = new BlobSasBuilder
        {
            BlobContainerName = pathResolver.ContainerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn),
        };

        builder.SetPermissions(permissions);

        return blobClient.GenerateSasUri(builder).ToString();
    }

    private static BlobSasPermissions ResolvePermissions(FilePermission permission)
    {
        var permissions = default(BlobSasPermissions);

        if (permission.HasFlag(FilePermission.Read))
        {
            permissions |= BlobSasPermissions.Read;
        }

        if (permission.HasFlag(FilePermission.Write))
        {
            permissions |= BlobSasPermissions.Write | BlobSasPermissions.Create;
        }

        if (permission.HasFlag(FilePermission.Delete))
        {
            permissions |= BlobSasPermissions.Delete;
        }

        if (permission.HasFlag(FilePermission.List))
        {
            permissions |= BlobSasPermissions.List;
        }

        return permissions == default ? BlobSasPermissions.Read : permissions;
    }
}
