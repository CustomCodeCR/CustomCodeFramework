using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Postgres.EntityFramework.Migrations;

public static class MigrationRunner
{
    public static async Task RunMigrationsAsync<TDbContext>(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
