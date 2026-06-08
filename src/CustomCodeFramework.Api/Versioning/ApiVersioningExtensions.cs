using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Api.Versioning;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddCustomCodeApiVersioning(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
