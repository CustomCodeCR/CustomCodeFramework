using System.Linq.Expressions;

namespace CustomCodeFramework.Persistence.Specifications;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>>? Criteria { get; }

    IReadOnlyCollection<Expression<Func<TEntity, object>>> Includes { get; }

    Expression<Func<TEntity, object>>? OrderBy { get; }

    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    int? Skip { get; }

    int? Take { get; }

    bool IsPagingEnabled { get; }

    bool AsNoTracking { get; }
}
