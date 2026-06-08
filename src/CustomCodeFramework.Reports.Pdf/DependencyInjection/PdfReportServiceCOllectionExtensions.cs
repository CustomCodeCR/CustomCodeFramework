using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Pdf.Exporting;
using CustomCodeFramework.Reports.Pdf.Rendering;
using CustomCodeFramework.Reports.Pdf.Templates;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Reports.Pdf.DependencyInjection;

public static class PdfReportServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodePdfReports(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<PdfExportOptions>()
            .Bind(configuration.GetSection(PdfExportOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultTitle),
                "PDF default title is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultAuthor),
                "PDF default author is required."
            )
            .ValidateOnStart();

        services
            .AddOptions<PdfRenderOptions>()
            .Bind(configuration.GetSection(PdfRenderOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.PaperSize),
                "PDF paper size is required."
            )
            .Validate(
                options => options.MarginTop >= 0,
                "PDF margin top must be greater than or equal to zero."
            )
            .Validate(
                options => options.MarginBottom >= 0,
                "PDF margin bottom must be greater than or equal to zero."
            )
            .Validate(
                options => options.MarginLeft >= 0,
                "PDF margin left must be greater than or equal to zero."
            )
            .Validate(
                options => options.MarginRight >= 0,
                "PDF margin right must be greater than or equal to zero."
            )
            .ValidateOnStart();

        services.AddSingleton<IConverter, SynchronizedConverter>(_ => new SynchronizedConverter(
            new PdfTools()
        ));

        services.AddScoped<PdfRenderer>();
        services.AddScoped<PdfTemplateRenderer>();
        services.AddScoped<IReportExporter, PdfReportExporter>();

        return services;
    }
}
