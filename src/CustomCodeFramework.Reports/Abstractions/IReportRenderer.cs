using CustomCodeFramework.Reports.Templates;

namespace CustomCodeFramework.Reports.Abstractions;

public interface IReportRenderer
{
    Task<string> RenderAsync(
        ReportTemplate template,
        ReportTemplateContext context,
        CancellationToken cancellationToken = default
    );
}
