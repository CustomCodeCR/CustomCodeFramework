using CustomCodeFramework.Mongo.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

namespace CustomCodeFramework.Mongo.HealthChecks;

public sealed class MongoHealthCheck(IMongoContext mongoContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var command = new BsonDocument("ping", 1);

            await mongoContext.Database.RunCommandAsync<BsonDocument>(
                command,
                cancellationToken: cancellationToken
            );

            return HealthCheckResult.Healthy("MongoDB is healthy.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB is unhealthy.", exception);
        }
    }
}
