using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Excel.Exporting;
using CustomCodeFramework.Reports.Excel.Worksheets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Reports.Excel.DependencyInjection;

public static class ExcelReportServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeExcelReports(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ExcelExportOptions>()
            .Bind(configuration.GetSection(ExcelExportOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultWorksheetName),
                "Excel default worksheet name is required."
            )
            .ValidateOnStart();

        services.AddScoped<ExcelWorksheetBuilder>();
        services.AddScoped<IReportExporter, ExcelReportExporter>();

        return services;
    }
}
