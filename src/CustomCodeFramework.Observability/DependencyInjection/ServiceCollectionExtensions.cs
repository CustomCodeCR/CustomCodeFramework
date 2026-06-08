using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Observability.Correlation;
using CustomCodeFramework.Observability.HealthChecks;
using CustomCodeFramework.Observability.Logging;
using CustomCodeFramework.Observability.Metrics;
using CustomCodeFramework.Observability.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Observability.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string? serviceVersion = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var otlpEndpoint = configuration["Observability:OtlpEndpoint"];

        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        services.AddScoped<ICorrelationContext, CorrelationContext>();

        services.AddCustomCodeSerilog(configuration, serviceName);
        services.AddCustomCodeTracing(serviceName, serviceVersion, otlpEndpoint);
        services.AddCustomCodeMetrics(serviceName, serviceVersion, otlpEndpoint);
        services.AddCustomCodeHealthChecks();

        return services;
    }
}
