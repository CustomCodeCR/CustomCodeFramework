using CustomCodeFramework.Mongo.Abstractions;
using MongoDB.Driver;

namespace CustomCodeFramework.Mongo.Repositories;

public class MongoRepository<TDocument>(IMongoCollectionProvider collectionProvider)
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

    public virtual async Task<IReadOnlyList<TDocument>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await Collection
            .Find(FilterDefinition<TDocument>.Empty)
            .ToListAsync(cancellationToken);
    }

    public virtual Task InsertAsync(
        TDocument document,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(document);

        return Collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public virtual Task ReplaceAsync(
        TDocument document,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(document);

        return Collection.ReplaceOneAsync(
            current => current.Id == document.Id,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken
        );
    }

    public virtual Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return Collection.DeleteOneAsync(document => document.Id == id, cancellationToken);
    }
}
