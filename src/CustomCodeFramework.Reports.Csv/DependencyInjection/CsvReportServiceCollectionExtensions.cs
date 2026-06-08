using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Csv.Exporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Reports.Csv.DependencyInjection;

public static class CsvReportServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeCsvReports(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<CsvExportOptions>()
            .Bind(configuration.GetSection(CsvExportOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<IReportExporter, CsvReportExporter>();

        return services;
    }
}
