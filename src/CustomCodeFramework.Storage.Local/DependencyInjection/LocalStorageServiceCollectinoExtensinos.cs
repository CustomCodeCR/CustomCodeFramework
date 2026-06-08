using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Local.Local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Storage.Local.DependencyInjection;

public static class LocalStorageServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeLocalStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<LocalStorageOptions>()
            .Bind(configuration.GetSection(LocalStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.RootPath),
                "Local storage root path is required."
            )
            .Validate(
                options =>
                    !options.UsePublicUrl || !string.IsNullOrWhiteSpace(options.PublicBaseUrl),
                "Local storage public base URL is required when public URL mode is enabled."
            )
            .ValidateOnStart();

        services.AddSingleton<LocalStoragePathResolver>();
        services.AddScoped<IFileStorageProvider, LocalFileStorage>();

        return services;
    }
}
