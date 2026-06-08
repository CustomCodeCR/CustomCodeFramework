namespace CustomCodeFramework.Postgres.Dapper.Abstractions;

public interface ISqlQueryExecutor
{
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    Task<T> QuerySingleAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );
}
