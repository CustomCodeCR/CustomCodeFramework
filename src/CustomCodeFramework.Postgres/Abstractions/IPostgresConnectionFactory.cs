using Npgsql;

namespace CustomCodeFramework.Postgres.Abstractions;

public interface IPostgresConnectionFactory
{
    Task<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
