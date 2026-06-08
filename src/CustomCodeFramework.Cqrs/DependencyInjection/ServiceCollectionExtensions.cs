using System.Reflection;
using CustomCodeFramework.Cqrs.Behaviors;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Cqrs.Dispatching;
using CustomCodeFramework.Cqrs.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Cqrs.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeCqrs(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        RegisterHandlers(services, assemblies);

        return services;
    }

    public static IServiceCollection AddCustomCodeCqrsBehaviors(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }

    private static void RegisterHandlers(
        IServiceCollection services,
        IReadOnlyCollection<Assembly> assemblies
    )
    {
        if (assemblies.Count == 0)
        {
            return;
        }

        var implementationTypes = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false }
                && type.ImplementedInterfaces.Any(IsCqrsHandlerInterface)
            )
            .ToArray();

        foreach (var implementationType in implementationTypes)
        {
            var handlerInterfaces = implementationType.ImplementedInterfaces.Where(
                IsCqrsHandlerInterface
            );

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, implementationType);
            }
        }
    }

    private static bool IsCqrsHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var genericDefinition = type.GetGenericTypeDefinition();

        return genericDefinition == typeof(ICommandHandler<>)
            || genericDefinition == typeof(ICommandHandler<,>)
            || genericDefinition == typeof(IQueryHandler<,>);
    }
}
