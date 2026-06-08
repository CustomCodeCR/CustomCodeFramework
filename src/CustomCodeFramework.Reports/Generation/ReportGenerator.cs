using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Options;
using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;
using CustomCodeFramework.Reports.Storage;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Generation;

public sealed class ReportGenerator(
    IEnumerable<IReportExporter> exporters,
    IEnumerable<IReportDataProvider> dataProviders,
    IReportTemplateProvider templateProvider,
    IReportStorage? reportStorage,
    IOptions<ReportsOptions> options
) : IReportGenerator
{
    public async Task<ReportGenerationResult> GenerateAsync(
        ReportGenerationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var exporter = exporters.FirstOrDefault(current => current.Format == request.Format);

        if (exporter is null)
        {
            return ReportGenerationResult.Failure(
                "report.exporter_not_registered",
                $"Report exporter for format '{request.Format}' is not registered."
            );
        }

        var dataProvider = dataProviders.FirstOrDefault(current =>
            current.ReportKey.Equals(request.ReportKey, StringComparison.OrdinalIgnoreCase)
        );

        if (dataProvider is null)
        {
            return ReportGenerationResult.Failure(
                "report.data_provider_not_registered",
                $"Report data provider for report '{request.ReportKey}' is not registered."
            );
        }

        var rows = await dataProvider.GetDataAsync(request, cancellationToken);

        if (rows.Count > options.Value.MaxRows)
        {
            return ReportGenerationResult.Failure(
                "report.max_rows_exceeded",
                $"Report row count exceeded the configured maximum of {options.Value.MaxRows}."
            );
        }

        var exportRequest = new ReportExportRequest
        {
            ReportKey = request.ReportKey,
            Parameters = request.Parameters,
            RequestedBy = request.RequestedBy,
            Format = request.Format,
            Rows = rows,
            FileName = BuildFileName(request.ReportKey, request.Format),
        };

        var file = await exporter.ExportAsync(exportRequest, cancellationToken);

        if (file.IsFailure)
        {
            return ReportGenerationResult.Failure(
                file.ErrorCode ?? "report.export_failed",
                file.ErrorMessage ?? "Report export failed."
            );
        }

        if (!request.StoreResult && !options.Value.StoreGeneratedReports)
        {
            return ReportGenerationResult.Success(file);
        }

        if (reportStorage is null)
        {
            return ReportGenerationResult.Success(file);
        }

        var storedFile = await reportStorage.SaveAsync(
            new ReportStoragePath
            {
                Folder = options.Value.DefaultStorageFolder,
                FileName = file.FileName,
            },
            file,
            cancellationToken
        );

        return ReportGenerationResult.Success(file, storedFile.Url, storedFile.Path);
    }

    private static string BuildFileName(string reportKey, ReportFormat format)
    {
        var normalizedReportKey = reportKey
            .Trim()
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        return $"{normalizedReportKey}-{DateTime.UtcNow:yyyyMMddHHmmss}{ReportFileExtension.FromFormat(format)}";
    }
}
