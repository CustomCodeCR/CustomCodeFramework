using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Word.Exporting;
using CustomCodeFramework.Reports.Word.Sections;
using CustomCodeFramework.Reports.Word.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Reports.Word.DependencyInjection;

public static class WordReportServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeWordReports(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<WordExportOptions>()
            .Bind(configuration.GetSection(WordExportOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultTitle),
                "Word default title is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DefaultAuthor),
                "Word default author is required."
            )
            .ValidateOnStart();

        services.AddScoped<WordSectionBuilder>();
        services.AddScoped<WordTemplateRenderer>();
        services.AddScoped<IReportExporter, WordReportExporter>();

        return services;
    }
}
