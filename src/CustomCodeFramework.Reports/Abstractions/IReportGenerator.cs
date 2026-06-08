using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;

namespace CustomCodeFramework.Reports.Abstractions;

public interface IReportGenerator
{
    Task<ReportGenerationResult> GenerateAsync(
        ReportGenerationRequest request,
        CancellationToken cancellationToken = default
    );
}
