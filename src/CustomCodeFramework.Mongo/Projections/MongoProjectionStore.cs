using CustomCodeFramework.Mongo.Abstractions;
using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.Projections;

public sealed class MongoProjectionStore(IMongoCollectionProvider collectionProvider)
{
    public IMongoCollection<TReadModel> Collection<TReadModel>()
    {
        return collectionProvider.GetCollection<TReadModel>();
    }

    public Task UpsertAsync<TReadModel>(
        string id,
        TReadModel readModel,
        CancellationToken cancellationToken = default
    )
        where TReadModel : IReadModel
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(readModel);

        var collection = collectionProvider.GetCollection<TReadModel>();

        return collection.ReplaceOneAsync(
            document => document.Id == id,
            readModel,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken
        );
    }

    public Task DeleteAsync<TReadModel>(string id, CancellationToken cancellationToken = default)
        where TReadModel : IReadModel
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var collection = collectionProvider.GetCollection<TReadModel>();

        return collection.DeleteOneAsync(document => document.Id == id, cancellationToken);
    }
}
