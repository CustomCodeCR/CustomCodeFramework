using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.Indexes;

public interface IMongoIndexBuilder
{
    Task CreateIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default);
}
