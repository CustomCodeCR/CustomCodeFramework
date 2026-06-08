using CustomCodeFramework.Postgres.EntityFramework.Abstractions;

namespace CustomCodeFramework.Postgres.EntityFramework.UnitOfWork;

public sealed class EfUnitOfWork(IDbContext dbContext) : IEfUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
