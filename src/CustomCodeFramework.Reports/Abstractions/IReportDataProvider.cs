using CustomCodeFramework.Reports.Requests;

namespace CustomCodeFramework.Reports.Abstractions;

public interface IReportDataProvider
{
    string ReportKey { get; }

    Task<IReadOnlyCollection<object>> GetDataAsync(
        ReportRequest request,
        CancellationToken cancellationToken = default
    );
}
