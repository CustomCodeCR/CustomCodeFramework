namespace CustomCodeFramework.ServiceDefaults.DependencyInjection;

public sealed class CustomCodeServiceDefaultsOptions
{
    public bool AddApi { get; init; } = true;

    public bool AddAuth { get; init; } = true;

    public bool AddAuthRedis { get; init; } = true;

    public bool AddPostgres { get; init; } = true;

    public bool AddMongo { get; init; }

    public bool AddRedis { get; init; } = true;

    public bool AddOutboxWorkers { get; init; }

    public bool AddRedisStreams { get; init; }
}
