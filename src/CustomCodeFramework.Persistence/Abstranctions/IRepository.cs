namespace CustomCodeFramework.Persistence.Abstractions;

public interface IRepository<TEntity, in TId>
    where TEntity : class
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
