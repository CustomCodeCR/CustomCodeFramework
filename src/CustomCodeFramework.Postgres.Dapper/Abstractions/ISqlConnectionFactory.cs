using System.Data;

namespace CustomCodeFramework.Postgres.Dapper.Abstractions;

public interface ISqlConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
