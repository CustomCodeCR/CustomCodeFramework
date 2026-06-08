using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Generation;
using CustomCodeFramework.Reports.Options;
using CustomCodeFramework.Reports.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Reports.DependencyInjection;

public static class ReportServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeReports(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ReportsOptions>()
            .Bind(configuration.GetSection(ReportsOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultStorageFolder),
                "Reports default storage folder is required."
            )
            .Validate(
                options => options.DefaultExpirationDays > 0,
                "Reports default expiration days must be greater than zero."
            )
            .Validate(options => options.MaxRows > 0, "Reports max rows must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<InMemoryReportTemplateProvider>();
        services.AddSingleton<IReportTemplateProvider>(provider =>
            provider.GetRequiredService<InMemoryReportTemplateProvider>()
        );

        services.AddScoped<IReportGenerator, ReportGenerator>();

        return services;
    }

    public static IServiceCollection AddReportExporter<TExporter>(this IServiceCollection services)
        where TExporter : class, IReportExporter
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IReportExporter, TExporter>();

        return services;
    }

    public static IServiceCollection AddReportDataProvider<TDataProvider>(
        this IServiceCollection services
    )
        where TDataProvider : class, IReportDataProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IReportDataProvider, TDataProvider>();

        return services;
    }
}
