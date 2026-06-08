namespace CustomCodeFramework.Postgres.Dapper.Pagination;

public static class SqlCountQueryBuilder
{
    public static string Build(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        return $"""
            select count(*)
            from (
                {sql.Trim().TrimEnd(';')}
            ) as count_query;
            """;
    }
}
