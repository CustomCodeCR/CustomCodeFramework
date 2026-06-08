using CustomCodeFramework.Reports.Csv.Formatting;

namespace CustomCodeFramework.Reports.Csv.Exporting;

public sealed class CsvExportOptions
{
    public const string SectionName = "Reports:Csv";

    public CsvDelimiter Delimiter { get; init; } = CsvDelimiter.Comma;

    public bool IncludeHeader { get; init; } = true;

    public bool UseUtf8Bom { get; init; } = true;
}
