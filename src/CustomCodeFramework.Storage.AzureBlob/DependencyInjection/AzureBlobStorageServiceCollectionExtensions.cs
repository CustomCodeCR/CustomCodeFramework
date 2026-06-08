using Azure.Storage.Blobs;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.AzureBlob.AzureBlob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Storage.AzureBlob.DependencyInjection;

public static class AzureBlobStorageServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeAzureBlobStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var azureBlobOptions =
            configuration
                .GetSection(AzureBlobStorageOptions.SectionName)
                .Get<AzureBlobStorageOptions>()
            ?? new AzureBlobStorageOptions();

        services
            .AddOptions<AzureBlobStorageOptions>()
            .Bind(configuration.GetSection(AzureBlobStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Azure Blob connection string is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ContainerName),
                "Azure Blob container name is required."
            )
            .Validate(
                options =>
                    !options.UsePublicUrl || !string.IsNullOrWhiteSpace(options.PublicBaseUrl),
                "Azure Blob public base URL is required when public URL mode is enabled."
            )
            .Validate(
                options => options.SignedUrlExpirationMinutes > 0,
                "Azure Blob signed URL expiration must be greater than zero."
            )
            .ValidateOnStart();

        services.AddSingleton(_ =>
        {
            var serviceClient = new BlobServiceClient(azureBlobOptions.ConnectionString);

            return serviceClient.GetBlobContainerClient(azureBlobOptions.ContainerName);
        });

        services.AddSingleton<AzureBlobPathResolver>();
        services.AddSingleton<AzureBlobSignedUrlGenerator>();
        services.AddScoped<IFileStorageProvider, AzureBlobFileStorage>();

        return services;
    }
}
