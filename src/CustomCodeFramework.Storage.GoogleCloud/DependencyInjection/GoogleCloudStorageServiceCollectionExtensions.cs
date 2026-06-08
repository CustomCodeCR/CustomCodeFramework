using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.GoogleCloud.GoogleCloud;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Storage.GoogleCloud.DependencyInjection;

public static class GoogleCloudStorageServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeGoogleCloudStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var googleCloudOptions =
            configuration
                .GetSection(GoogleCloudStorageOptions.SectionName)
                .Get<GoogleCloudStorageOptions>()
            ?? new GoogleCloudStorageOptions();

        services
            .AddOptions<GoogleCloudStorageOptions>()
            .Bind(configuration.GetSection(GoogleCloudStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BucketName),
                "Google Cloud bucket name is required."
            )
            .Validate(
                options =>
                    !options.CreateBucketIfNotExists
                    || !string.IsNullOrWhiteSpace(options.ProjectId),
                "Google Cloud project ID is required when bucket creation is enabled."
            )
            .Validate(
                options =>
                    !options.UsePublicUrl || !string.IsNullOrWhiteSpace(options.PublicBaseUrl),
                "Google Cloud public base URL is required when public URL mode is enabled."
            )
            .Validate(
                options => options.SignedUrlExpirationMinutes > 0,
                "Google Cloud signed URL expiration must be greater than zero."
            )
            .ValidateOnStart();

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(googleCloudOptions.CredentialFilePath))
            {
                var credential = GoogleCredential.FromFile(googleCloudOptions.CredentialFilePath);

                return StorageClient.Create(credential);
            }

            return StorageClient.Create();
        });

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(googleCloudOptions.CredentialFilePath))
            {
                return UrlSigner.FromCredential(
                    GoogleCredential.FromFile(googleCloudOptions.CredentialFilePath)
                );
            }

            return UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
        });

        services.AddSingleton<GoogleCloudPathResolver>();
        services.AddSingleton<GoogleCloudSignedUrlGenerator>();
        services.AddScoped<IFileStorageProvider, GoogleCloudFileStorage>();

        return services;
    }
}
