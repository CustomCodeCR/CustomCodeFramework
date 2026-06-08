using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.S3.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Storage.S3.DependencyInjection;

public static class S3StorageServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeS3Storage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var s3Options =
            configuration.GetSection(S3StorageOptions.SectionName).Get<S3StorageOptions>()
            ?? new S3StorageOptions();

        services
            .AddOptions<S3StorageOptions>()
            .Bind(configuration.GetSection(S3StorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BucketName),
                "S3 bucket name is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Region),
                "S3 region is required."
            )
            .Validate(
                options =>
                    string.IsNullOrWhiteSpace(options.AccessKey)
                    == string.IsNullOrWhiteSpace(options.SecretKey),
                "Both S3 access key and secret key must be provided together."
            )
            .Validate(
                options =>
                    !options.UsePublicUrl
                    || !string.IsNullOrWhiteSpace(options.PublicBaseUrl)
                    || !string.IsNullOrWhiteSpace(options.ServiceUrl),
                "S3 public base URL or service URL is required when public URL mode is enabled."
            )
            .Validate(
                options => options.SignedUrlExpirationMinutes > 0,
                "S3 signed URL expiration must be greater than zero."
            )
            .ValidateOnStart();

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(s3Options.Region),
                ForcePathStyle = s3Options.ForcePathStyle,
            };

            if (!string.IsNullOrWhiteSpace(s3Options.ServiceUrl))
            {
                config.ServiceURL = s3Options.ServiceUrl;
                config.UseHttp = s3Options.ServiceUrl.StartsWith(
                    "http://",
                    StringComparison.OrdinalIgnoreCase
                );
            }

            if (
                !string.IsNullOrWhiteSpace(s3Options.AccessKey)
                && !string.IsNullOrWhiteSpace(s3Options.SecretKey)
            )
            {
                var credentials = new BasicAWSCredentials(s3Options.AccessKey, s3Options.SecretKey);

                return new AmazonS3Client(credentials, config);
            }

            return new AmazonS3Client(config);
        });

        services.AddSingleton<S3StoragePathResolver>();
        services.AddSingleton<S3SignedUrlGenerator>();
        services.AddScoped<IFileStorageProvider, S3FileStorage>();

        return services;
    }
}
