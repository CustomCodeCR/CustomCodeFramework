using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.DbContexts;

public static class DbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseCustomCodePostgres(
        this DbContextOptionsBuilder builder,
        string connectionString,
        int commandTimeoutSeconds = 30
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return builder.UseNpgsql(
            connectionString,
            options =>
            {
                options.CommandTimeout(commandTimeoutSeconds);
                options.EnableRetryOnFailure();
            }
        );
    }
}
