using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;
using CustomCodeFramework.Mongo.HealthChecks;
using CustomCodeFramework.Mongo.Indexes;
using CustomCodeFramework.Mongo.Options;
using CustomCodeFramework.Mongo.Projections;
using CustomCodeFramework.Mongo.Repositories;
using CustomCodeFramework.Mongo.Transactions;
using CustomCodeFramework.Persistence.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeMongo(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<MongoOptions>()
            .Bind(configuration.GetSection(MongoOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Mongo connection string is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DatabaseName),
                "Mongo database name is required."
            )
            .ValidateOnStart();

        var options =
            configuration.GetSection(MongoOptions.SectionName).Get<MongoOptions>()
            ?? new MongoOptions();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(options.ConnectionString));

        services.AddSingleton<IMongoContext, MongoContext>();
        services.AddSingleton<IMongoCollectionProvider>(provider =>
            provider.GetRequiredService<MongoContext>()
        );

        services.AddScoped(typeof(MongoRepository<>));
        services.AddScoped(typeof(MongoReadRepository<>));
        services.AddScoped<MongoProjectionStore>();
        services.AddScoped<MongoIndexRunner>();
        services.AddScoped<ITransactionManager, MongoTransactionManager>();

        if (options.EnableHealthCheck)
        {
            services
                .AddHealthChecks()
                .AddCheck<MongoHealthCheck>("mongo", tags: ["database", "mongo"]);
        }

        return services;
    }
}

internal sealed class MongoContext(
    IMongoClient mongoClient,
    Microsoft.Extensions.Options.IOptions<MongoOptions> options
) : IMongoContext, IMongoCollectionProvider
{
    public IMongoDatabase Database { get; } = mongoClient.GetDatabase(options.Value.DatabaseName);

    public IMongoClient Client { get; } = mongoClient;

    public IMongoCollection<TDocument> GetCollection<TDocument>()
    {
        var collectionName = MongoCollectionRegistry.GetCollectionName<TDocument>();

        return GetCollection<TDocument>(collectionName);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        return Database.GetCollection<TDocument>(collectionName);
    }
}
