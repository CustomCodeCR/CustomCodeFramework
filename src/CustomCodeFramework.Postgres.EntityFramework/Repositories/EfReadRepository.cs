using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Persistence.Specifications;
using CustomCodeFramework.Postgres.EntityFramework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.Repositories;

public class EfReadRepository<TEntity, TId>(IDbContext dbContext) : IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    public virtual Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        return DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    public virtual Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public virtual Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return ApplySpecification(specification).CountAsync(cancellationToken);
    }

    public virtual Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    protected IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), specification);
    }
}
