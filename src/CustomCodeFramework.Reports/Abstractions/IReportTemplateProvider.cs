using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Templates;

namespace CustomCodeFramework.Reports.Abstractions;

public interface IReportTemplateProvider
{
    Task<ReportTemplate?> GetTemplateAsync(
        string templateKey,
        ReportFormat format,
        CancellationToken cancellationToken = default
    );
}
