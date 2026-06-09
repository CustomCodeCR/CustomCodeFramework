using CustomCodeFramework.Workers.Abstractions;
using CustomCodeFramework.Workers.HostedServices;
using CustomCodeFramework.Workers.Options;
using CustomCodeFramework.Workers.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Workers.DependencyInjection;

public static class WorkerServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeWorkers(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<WorkerOptions>(configuration.GetSection("Workers"));

        services.Configure<WorkerScheduleOptions>(configuration.GetSection("Workers:Schedule"));

        return services;
    }

    public static IServiceCollection AddCustomCodeWorker<TWorker>(this IServiceCollection services)
        where TWorker : class, IBackgroundWorker
    {
        services.AddScoped<TWorker>();
        services.AddHostedService<BackgroundWorkerService<TWorker>>();

        return services;
    }

    public static IServiceCollection AddCustomCodePeriodicWorker<TWorker>(
        this IServiceCollection services
    )
        where TWorker : class, IBackgroundWorker
    {
        services.AddScoped<TWorker>();
        services.AddHostedService<PeriodicBackgroundService<TWorker>>();

        return services;
    }

    public static IServiceCollection AddCustomCodeResilientWorker<TWorker>(
        this IServiceCollection services
    )
        where TWorker : class, IBackgroundWorker
    {
        services.AddScoped<TWorker>();
        services.AddHostedService<ResilientBackgroundService<TWorker>>();

        return services;
    }

    public static IServiceCollection AddCustomCodeWorkerLockProvider<TProvider>(
        this IServiceCollection services
    )
        where TProvider : class, IWorkerLockProvider
    {
        services.AddScoped<IWorkerLockProvider, TProvider>();

        return services;
    }
}
