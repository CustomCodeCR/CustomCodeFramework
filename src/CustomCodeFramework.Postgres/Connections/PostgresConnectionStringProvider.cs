using CustomCodeFramework.Postgres.Abstractions;
using CustomCodeFramework.Postgres.Options;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Postgres.Connections;

public sealed class PostgresConnectionStringProvider(IOptions<PostgresOptions> options)
    : IPostgresConnectionStringProvider
{
    public string GetConnectionString()
    {
        return options.Value.ConnectionString;
    }
}
