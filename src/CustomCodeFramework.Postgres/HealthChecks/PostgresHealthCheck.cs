using CustomCodeFramework.Postgres.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomCodeFramework.Postgres.HealthChecks;

public sealed class PostgresHealthCheck(IPostgresConnectionFactory connectionFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken
            );

            await using var command = connection.CreateCommand();
            command.CommandText = "select 1;";

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result is 1
                ? HealthCheckResult.Healthy("PostgreSQL is healthy.")
                : HealthCheckResult.Unhealthy(
                    "PostgreSQL health check returned an invalid result."
                );
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is unhealthy.", exception);
        }
    }
}
