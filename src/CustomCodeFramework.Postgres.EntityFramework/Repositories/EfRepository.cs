using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Postgres.EntityFramework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.Repositories;

public class EfRepository<TEntity, TId>(IDbContext dbContext) : IEfRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected readonly IDbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    public virtual Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        return DbSet.FindAsync([id], cancellationToken).AsTask();
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}
