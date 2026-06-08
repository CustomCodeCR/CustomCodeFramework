using System.Reflection;
using CustomCodeFramework.Validation.Abstractions;
using CustomCodeFramework.Validation.FluentValidation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Validation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeValidation(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IValidationService, FluentValidationService>();

        if (assemblies.Length > 0)
        {
            services.AddValidatorsFromAssemblies(assemblies);
        }

        return services;
    }
}
