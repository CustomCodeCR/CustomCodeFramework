using CustomCodeFramework.Postgres.Dapper.Abstractions;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.Executors;

public sealed class SqlQueryExecutor(ISqlConnectionFactory connectionFactory) : ISqlQueryExecutor
{
    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<T> QuerySingleAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<T>(command);
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<T>(command);

        return result.AsList();
    }
}
