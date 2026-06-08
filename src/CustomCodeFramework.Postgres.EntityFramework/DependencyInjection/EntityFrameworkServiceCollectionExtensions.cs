using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Postgres.Abstractions;
using CustomCodeFramework.Postgres.EntityFramework.Abstractions;
using CustomCodeFramework.Postgres.EntityFramework.Interceptors;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using CustomCodeFramework.Postgres.EntityFramework.Transactions;
using CustomCodeFramework.Postgres.EntityFramework.UnitOfWork;
using CustomCodeFramework.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;

public static class EntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodePostgresEntityFramework<TDbContext>(
        this IServiceCollection services
    )
        where TDbContext : DbContext, IDbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventInterceptor>();
        services.AddScoped<OutboxSaveChangesInterceptor>();
        services.AddScoped<ConcurrencyInterceptor>();

        services.AddDbContext<TDbContext>(
            (serviceProvider, optionsBuilder) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;
                var connectionStringProvider =
                    serviceProvider.GetRequiredService<IPostgresConnectionStringProvider>();

                optionsBuilder.UseNpgsql(
                    connectionStringProvider.GetConnectionString(),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(options.CommandTimeoutSeconds);
                        npgsqlOptions.EnableRetryOnFailure();
                    }
                );

                optionsBuilder.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
                    serviceProvider.GetRequiredService<SoftDeleteInterceptor>(),
                    serviceProvider.GetRequiredService<DomainEventInterceptor>(),
                    serviceProvider.GetRequiredService<OutboxSaveChangesInterceptor>(),
                    serviceProvider.GetRequiredService<ConcurrencyInterceptor>()
                );
            }
        );

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<TDbContext>());
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TDbContext>());

        services.AddScoped<IEfUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IEfUnitOfWork>());

        services.AddScoped<ITransactionManager, EfTransactionManager>();

        services.AddScoped(typeof(IEfRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));

        return services;
    }
}
