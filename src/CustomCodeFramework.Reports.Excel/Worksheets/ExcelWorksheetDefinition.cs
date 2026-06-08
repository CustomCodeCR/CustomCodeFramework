using CustomCodeFramework.Reports.Definitions;
using CustomCodeFramework.Reports.Excel.Styling;

namespace CustomCodeFramework.Reports.Excel.Worksheets;

public sealed record ExcelWorksheetDefinition
{
    public required string Name { get; init; }

    public IReadOnlyCollection<object> Rows { get; init; } = [];

    public IReadOnlyCollection<ReportColumnDefinition> Columns { get; init; } = [];

    public IReadOnlyCollection<ExcelColumnStyle> ColumnStyles { get; init; } = [];

    public ExcelStyleOptions StyleOptions { get; init; } = new();
}
