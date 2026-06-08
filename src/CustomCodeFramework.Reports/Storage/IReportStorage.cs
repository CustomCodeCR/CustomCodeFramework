using CustomCodeFramework.Reports.Results;

namespace CustomCodeFramework.Reports.Storage;

public interface IReportStorage
{
    Task<ReportStoredFile> SaveAsync(
        ReportStoragePath path,
        ReportFileResult file,
        CancellationToken cancellationToken = default
    );

    Task<ReportFileResult?> GetAsync(string path, CancellationToken cancellationToken = default);

    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
