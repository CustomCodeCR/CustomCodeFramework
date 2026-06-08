using System.Data;
using CustomCodeFramework.Postgres.Dapper.Abstractions;

namespace CustomCodeFramework.Postgres.Dapper.Connections;

public sealed class DapperConnectionFactory(ISqlConnectionFactory connectionFactory)
{
    public Task<IDbConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        return connectionFactory.CreateOpenConnectionAsync(cancellationToken);
    }
}
