using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;

namespace CustomCodeFramework.Reports.Abstractions;

public interface IReportExporter
{
    ReportFormat Format { get; }

    Task<ReportFileResult> ExportAsync(
        ReportExportRequest request,
        CancellationToken cancellationToken = default
    );
}
