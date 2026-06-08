namespace CustomCodeFramework.Postgres.Options;

public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    public string ConnectionString { get; init; } = string.Empty;

    public bool EnableHealthCheck { get; init; } = true;

    public int CommandTimeoutSeconds { get; init; } = 30;
}
