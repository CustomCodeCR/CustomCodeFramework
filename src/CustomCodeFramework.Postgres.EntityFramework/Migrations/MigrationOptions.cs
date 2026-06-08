namespace CustomCodeFramework.Postgres.EntityFramework.Migrations;

public sealed record MigrationOptions
{
    public bool RunMigrationsOnStartup { get; init; }

    public int TimeoutSeconds { get; init; } = 60;
}
