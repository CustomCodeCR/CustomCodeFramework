using CustomCodeFramework.Persistence.Abstractions;

namespace CustomCodeFramework.Postgres.EntityFramework.Abstractions;

public interface IEfRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : class
    where TId : notnull { }
