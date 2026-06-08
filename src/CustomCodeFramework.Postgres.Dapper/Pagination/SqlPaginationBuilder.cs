using CustomCodeFramework.Postgres.Dapper.Abstractions;

namespace CustomCodeFramework.Postgres.Dapper.Pagination;

public sealed class SqlPaginationBuilder : ISqlPaginationBuilder
{
    public string ApplyPagination(string sql, SqlPagination pagination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(pagination);

        return $"""
            {sql.Trim().TrimEnd(';')}
            limit @PageSize offset @Offset;
            """;
    }

    public string BuildCountQuery(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        return SqlCountQueryBuilder.Build(sql);
    }
}
