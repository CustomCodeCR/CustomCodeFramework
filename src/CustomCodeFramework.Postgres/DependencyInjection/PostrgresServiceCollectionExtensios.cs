using CustomCodeFramework.Postgres.Abstractions;
using CustomCodeFramework.Postgres.Connections;
using CustomCodeFramework.Postgres.HealthChecks;
using CustomCodeFramework.Postgres.Options;
using CustomCodeFramework.Postgres.Resilience;
using CustomCodeFramework.Postgres.Scripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Postgres.DependencyInjection;

public static class PostgresServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodePostgres(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<PostgresOptions>()
            .Bind(configuration.GetSection(PostgresOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PostgresOptions>, PostgresOptionsValidator>();

        services
            .AddOptions<PostgresRetryOptions>()
            .Bind(configuration.GetSection(PostgresRetryOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<
            IPostgresConnectionStringProvider,
            PostgresConnectionStringProvider
        >();
        services.AddScoped<IPostgresConnectionFactory, PostgresConnectionFactory>();
        services.AddScoped<ISqlScriptRunner, SqlScriptRunner>();

        var postgresOptions =
            configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()
            ?? new PostgresOptions();

        if (postgresOptions.EnableHealthCheck)
        {
            services.AddHealthChecks().AddCustomCodePostgresHealthCheck();
        }

        return services;
    }
}
