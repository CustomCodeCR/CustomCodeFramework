namespace CustomCodeFramework.Postgres.Dapper.Abstractions;

public interface ISqlCommandExecutor
{
    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );
}
