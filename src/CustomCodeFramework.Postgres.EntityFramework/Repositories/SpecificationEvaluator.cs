using CustomCodeFramework.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.Repositories;

public static class SpecificationEvaluator
{
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> specification
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(inputQuery);
        ArgumentNullException.ThrowIfNull(specification);

        var query = inputQuery;

        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include)
        );

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip.GetValueOrDefault())
                .Take(specification.Take.GetValueOrDefault());
        }

        return query;
    }
}
