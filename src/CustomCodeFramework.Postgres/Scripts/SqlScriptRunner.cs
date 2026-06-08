using CustomCodeFramework.Postgres.Abstractions;
using CustomCodeFramework.Postgres.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CustomCodeFramework.Postgres.Scripts;

public sealed class SqlScriptRunner(
    IPostgresConnectionFactory connectionFactory,
    IOptions<PostgresOptions> options
) : ISqlScriptRunner
{
    public async Task ExecuteAsync(string sql, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken
        );

        await using var command = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = options.Value.CommandTimeoutSeconds,
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ExecuteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("SQL script file was not found.", filePath);
        }

        var sql = await File.ReadAllTextAsync(filePath, cancellationToken);

        await ExecuteAsync(sql, cancellationToken);
    }
}
