using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomCodeFramework.Postgres.HealthChecks;

public static class PostgresHealthCheckExtensions
{
    public static IHealthChecksBuilder AddCustomCodePostgresHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "postgres"
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCheck<PostgresHealthCheck>(
            name,
            failureStatus: HealthStatus.Unhealthy,
            tags: ["database", "postgres"]
        );
    }
}
