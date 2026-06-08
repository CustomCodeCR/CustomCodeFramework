using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Postgres.Dapper.Abstractions;
using CustomCodeFramework.Postgres.Dapper.Connections;
using CustomCodeFramework.Postgres.Dapper.Executors;
using CustomCodeFramework.Postgres.Dapper.Pagination;
using CustomCodeFramework.Postgres.Dapper.Transactions;
using CustomCodeFramework.Postgres.Dapper.TypeHandlers;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Postgres.Dapper.DependencyInjection;

public static class DapperServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodePostgresDapper(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        RegisterTypeHandlers();

        services.AddScoped<ISqlConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<DapperConnectionFactory>();

        services.AddScoped<ISqlQueryExecutor, SqlQueryExecutor>();
        services.AddScoped<ISqlCommandExecutor, SqlCommandExecutor>();
        services.AddScoped<PaginatedSqlQueryExecutor>();

        services.AddScoped<ISqlPaginationBuilder, SqlPaginationBuilder>();

        services.AddScoped<ITransactionManager, DapperTransactionManager>();

        return services;
    }

    private static void RegisterTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
    }
}
