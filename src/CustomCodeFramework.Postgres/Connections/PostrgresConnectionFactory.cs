using CustomCodeFramework.Postgres.Abstractions;
using Npgsql;

namespace CustomCodeFramework.Postgres.Connections;

public sealed class PostgresConnectionFactory(
    IPostgresConnectionStringProvider connectionStringProvider
) : IPostgresConnectionFactory
{
    public async Task<NpgsqlConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connection = new NpgsqlConnection(connectionStringProvider.GetConnectionString());

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
