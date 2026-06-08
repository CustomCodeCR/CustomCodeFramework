using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.HealthChecks;

public sealed class RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = connectionMultiplexer.GetDatabase();
            var result = await database.PingAsync();

            return result.TotalMilliseconds >= 0
                ? HealthCheckResult.Healthy("Redis is healthy.")
                : HealthCheckResult.Unhealthy("Redis ping returned an invalid result.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis is unhealthy.", exception);
        }
    }
}
