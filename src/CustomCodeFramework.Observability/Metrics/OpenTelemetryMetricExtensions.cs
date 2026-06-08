using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace CustomCodeFramework.Observability.Metrics;

public static class OpenTelemetryMetricsExtensions
{
    public static IServiceCollection AddCustomCodeMetrics(
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
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(CustomCodeMeterProvider.DefaultMeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }
}
