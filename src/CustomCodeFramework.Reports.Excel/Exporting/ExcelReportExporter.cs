using ClosedXML.Excel;
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Excel.Worksheets;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Excel.Exporting;

public sealed class ExcelReportExporter(
    ExcelWorksheetBuilder worksheetBuilder,
    IOptions<ExcelExportOptions> options
) : IReportExporter
{
    public ReportFormat Format => ReportFormat.Excel;

    public Task<ReportFileResult> ExportAsync(
        ReportExportRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using var workbook = new XLWorkbook();

            worksheetBuilder.BuildWorksheet(
                workbook,
                new ExcelWorksheetDefinition
                {
                    Name = options.Value.DefaultWorksheetName,
                    Rows = request.Rows,
                    Columns = request.Columns,
                }
            );

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var content = stream.ToArray();

            var fileName = ResolveFileName(request);

            var result = ReportFileResult.Success(
                fileName,
                ReportContentType.Excel,
                content,
                ReportFormat.Excel
            );

            return Task.FromResult(result);
        }
        catch (Exception exception)
        {
            var result = ReportFileResult.Failure("excel.export_failed", exception.Message);

            return Task.FromResult(result);
        }
    }

    private static string ResolveFileName(ReportExportRequest request)
    {
        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? $"{request.ReportKey}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
            : request.FileName;

        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".xlsx";
        }

        return fileName;
    }
}
