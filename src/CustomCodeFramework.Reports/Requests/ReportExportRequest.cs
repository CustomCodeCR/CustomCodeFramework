using CustomCodeFramework.Reports.Formats;

namespace CustomCodeFramework.Reports.Requests;

public sealed record ReportExportRequest : ReportRequest
{
    public ReportFormat Format { get; init; } = ReportFormat.Excel;

    public IReadOnlyCollection<object> Rows { get; init; } = [];

    public IReadOnlyCollection<Definitions.ReportColumnDefinition> Columns { get; init; } = [];

    public string? FileName { get; init; }
}
