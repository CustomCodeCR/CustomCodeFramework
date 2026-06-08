using CustomCodeFramework.Postgres.EntityFramework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.DbContexts;

public abstract class AppDbContextBase(DbContextOptions options) : DbContext(options), IDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyCustomCodeConventions();

        base.OnModelCreating(modelBuilder);
    }
}
