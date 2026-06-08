using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Firebase.Firebase;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Storage.Firebase.DependencyInjection;

public static class FirebaseStorageServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeFirebaseStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var firebaseOptions =
            configuration
                .GetSection(FirebaseStorageOptions.SectionName)
                .Get<FirebaseStorageOptions>()
            ?? new FirebaseStorageOptions();

        services
            .AddOptions<FirebaseStorageOptions>()
            .Bind(configuration.GetSection(FirebaseStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BucketName),
                "Firebase storage bucket name is required."
            )
            .Validate(
                options =>
                    !options.UsePublicUrl
                    || !string.IsNullOrWhiteSpace(options.PublicBaseUrl)
                    || options.UseFirebaseDownloadToken,
                "Firebase public base URL or download token mode is required when public URL mode is enabled."
            )
            .Validate(
                options => options.SignedUrlExpirationMinutes > 0,
                "Firebase signed URL expiration must be greater than zero."
            )
            .ValidateOnStart();

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(firebaseOptions.CredentialFilePath))
            {
                var credential = GoogleCredential.FromFile(firebaseOptions.CredentialFilePath);

                return StorageClient.Create(credential);
            }

            return StorageClient.Create();
        });

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(firebaseOptions.CredentialFilePath))
            {
                return UrlSigner.FromCredential(
                    GoogleCredential.FromFile(firebaseOptions.CredentialFilePath)
                );
            }

            return UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
        });

        services.AddSingleton<FirebasePathResolver>();
        services.AddSingleton<FirebaseSignedUrlGenerator>();
        services.AddScoped<IFileStorageProvider, FirebaseFileStorage>();

        return services;
    }
}
