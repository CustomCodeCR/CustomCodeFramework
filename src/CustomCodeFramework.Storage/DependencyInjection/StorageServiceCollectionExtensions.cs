using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.ContentTypes;
using CustomCodeFramework.Storage.Options;
using CustomCodeFramework.Storage.Paths;
using CustomCodeFramework.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Storage.DependencyInjection;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var storageOptions =
            configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>()
            ?? new StorageOptions();

        services.AddSingleton(storageOptions);

        services
            .AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ProviderName),
                "Storage provider name is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultFolder),
                "Storage default folder is required."
            )
            .Validate(
                options => options.MaxFileSizeInBytes > 0,
                "Storage max file size must be greater than zero."
            )
            .ValidateOnStart();

        services.AddSingleton<IFileNameGenerator, DefaultFileNameGenerator>();
        services.AddSingleton<IContentTypeProvider, ContentTypeMap>();
        services.AddSingleton<IStorageUrlGenerator, DefaultStorageUrlGenerator>();
        services.AddSingleton<IFilePathBuilder, StoragePathBuilder>();

        services.AddScoped<IFileStorage, FileStorage>();

        return services;
    }

    public static IServiceCollection AddFileStorageProvider<TProvider>(
        this IServiceCollection services
    )
        where TProvider : class, IFileStorageProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IFileStorageProvider, TProvider>();

        return services;
    }
}
