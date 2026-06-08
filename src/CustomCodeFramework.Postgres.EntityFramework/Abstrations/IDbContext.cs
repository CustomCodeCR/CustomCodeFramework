using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CustomCodeFramework.Postgres.EntityFramework.Abstractions;

public interface IDbContext
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
