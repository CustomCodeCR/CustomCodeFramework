using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CustomCodeFramework.Observability.Tracing;

public static class OpenTelemetryTracingExtensions
{
    public static IServiceCollection AddCustomCodeTracing(
        this IServiceCollection services,
        string serviceName,
        string? serviceVersion = null,
        string? otlpEndpoint = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ActivitySources.DefaultSourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }
}
