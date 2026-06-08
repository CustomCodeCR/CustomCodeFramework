namespace CustomCodeFramework.Reports.Pdf.Exporting;

public sealed class PdfExportOptions
{
    public const string SectionName = "Reports:Pdf";

    public string DefaultTitle { get; init; } = "Report";

    public string DefaultAuthor { get; init; } = "CustomCodeFramework";

    public bool IncludeGeneratedAt { get; init; } = true;

    public bool IncludeParameters { get; init; } = true;

    public bool IncludeTableHeader { get; init; } = true;

    public bool UseLandscape { get; init; } = false;
}
