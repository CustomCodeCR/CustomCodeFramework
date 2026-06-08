using CustomCodeFramework.Persistence.Specifications;

namespace CustomCodeFramework.Persistence.Abstractions;

public interface IReadRepository<TEntity, in TId>
    where TEntity : class
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );

    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );
}
