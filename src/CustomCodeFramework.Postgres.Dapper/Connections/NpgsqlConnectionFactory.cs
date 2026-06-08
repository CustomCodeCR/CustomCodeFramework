using System.Data;
using CustomCodeFramework.Postgres.Abstractions;
using CustomCodeFramework.Postgres.Dapper.Abstractions;

namespace CustomCodeFramework.Postgres.Dapper.Connections;

public sealed class NpgsqlConnectionFactory(IPostgresConnectionFactory postgresConnectionFactory)
    : ISqlConnectionFactory
{
    public async Task<IDbConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await postgresConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
    }
}
