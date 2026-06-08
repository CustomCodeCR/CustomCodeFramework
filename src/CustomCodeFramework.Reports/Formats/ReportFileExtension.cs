namespace CustomCodeFramework.Reports.Formats;

public static class ReportFileExtension
{
    public const string Pdf = ".pdf";
    public const string Excel = ".xlsx";
    public const string Csv = ".csv";
    public const string Word = ".docx";
    public const string Html = ".html";
    public const string Json = ".json";

    public static string FromFormat(ReportFormat format)
    {
        return format switch
        {
            ReportFormat.Pdf => Pdf,
            ReportFormat.Excel => Excel,
            ReportFormat.Csv => Csv,
            ReportFormat.Word => Word,
            ReportFormat.Html => Html,
            ReportFormat.Json => Json,
            _ => throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                "Unsupported report format."
            ),
        };
    }
}
