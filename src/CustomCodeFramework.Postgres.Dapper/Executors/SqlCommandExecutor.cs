using CustomCodeFramework.Postgres.Dapper.Abstractions;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.Executors;

public sealed class SqlCommandExecutor(ISqlConnectionFactory connectionFactory)
    : ISqlCommandExecutor
{
    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }
}
