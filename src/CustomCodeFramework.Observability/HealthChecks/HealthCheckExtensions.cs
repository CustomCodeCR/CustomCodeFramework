using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Observability.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomCodeHealthChecks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHealthChecks();

        return services;
    }

    public static IApplicationBuilder UseCustomCodeHealthChecks(
        this IApplicationBuilder app,
        string path = "/health"
    )
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        app.UseHealthChecks(
            path,
            new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter.WriteAsync }
        );

        return app;
    }
}
