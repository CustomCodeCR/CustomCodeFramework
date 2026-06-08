using CustomCodeFramework.Postgres.Dapper.Pagination;

namespace CustomCodeFramework.Postgres.Dapper.Abstractions;

public interface ISqlPaginationBuilder
{
    string ApplyPagination(string sql, SqlPagination pagination);

    string BuildCountQuery(string sql);
}
