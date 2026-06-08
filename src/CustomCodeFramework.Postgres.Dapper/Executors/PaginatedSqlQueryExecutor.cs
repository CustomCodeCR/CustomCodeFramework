using CustomCodeFramework.Postgres.Dapper.Abstractions;
using CustomCodeFramework.Postgres.Dapper.Pagination;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.Executors;

public sealed class PaginatedSqlQueryExecutor(
    ISqlConnectionFactory connectionFactory,
    ISqlPaginationBuilder paginationBuilder
)
{
    public async Task<SqlPagedResult<T>> QueryPagedAsync<T>(
        string sql,
        SqlPagination pagination,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(pagination);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var countSql = paginationBuilder.BuildCountQuery(sql);
        var pagedSql = paginationBuilder.ApplyPagination(sql, pagination);

        var queryParameters = MergePaginationParameters(parameters, pagination);

        var totalCount = await connection.QuerySingleAsync<long>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken)
        );

        var items = await connection.QueryAsync<T>(
            new CommandDefinition(pagedSql, queryParameters, cancellationToken: cancellationToken)
        );

        return SqlPagedResult<T>.Create(
            items.AsList(),
            pagination.PageNumber,
            pagination.PageSize,
            totalCount
        );
    }

    private static object MergePaginationParameters(object? parameters, SqlPagination pagination)
    {
        var dynamicParameters = new DynamicParameters(parameters);
        dynamicParameters.Add("PageSize", pagination.PageSize);
        dynamicParameters.Add("Offset", pagination.Offset);

        return dynamicParameters;
    }
}
