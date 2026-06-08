namespace CustomCodeFramework.Reports.Excel.Exporting;

public sealed class ExcelExportOptions
{
    public const string SectionName = "Reports:Excel";

    public string DefaultWorksheetName { get; init; } = "Report";

    public bool AutoAdjustColumns { get; init; } = true;

    public bool FreezeHeaderRow { get; init; } = true;

    public bool UseTable { get; init; } = true;

    public bool IncludeHeader { get; init; } = true;
}
