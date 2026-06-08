using CustomCodeFramework.Mongo.Abstractions;
using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.Repositories;

public class MongoReadRepository<TDocument>(IMongoCollectionProvider collectionProvider)
    where TDocument : IReadModel
{
    protected readonly IMongoCollection<TDocument> Collection =
        collectionProvider.GetCollection<TDocument>();

    public virtual async Task<TDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return await Collection
            .Find(document => document.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TDocument>> FindAsync(
        FilterDefinition<TDocument> filter,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }

    public virtual Task<long> CountAsync(
        FilterDefinition<TDocument> filter,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        return Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
