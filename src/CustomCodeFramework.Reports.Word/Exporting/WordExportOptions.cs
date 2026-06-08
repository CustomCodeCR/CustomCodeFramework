namespace CustomCodeFramework.Reports.Word.Exporting;

public sealed class WordExportOptions
{
    public const string SectionName = "Reports:Word";

    public string DefaultTitle { get; init; } = "Report";

    public string DefaultAuthor { get; init; } = "CustomCodeFramework";

    public bool IncludeGeneratedAt { get; init; } = true;

    public bool IncludeParameters { get; init; } = true;

    public bool IncludeTableHeader { get; init; } = true;
}
