using CustomCodeFramework.Postgres.EntityFramework.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;

public static class EntityFrameworkApplicationBuilderExtensions
{
    public static async Task<IHost> UseCustomCodePostgresMigrationsAsync<TDbContext>(
        this IHost host,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(host);

        await MigrationRunner.RunMigrationsAsync<TDbContext>(host.Services, cancellationToken);

        return host;
    }
}
