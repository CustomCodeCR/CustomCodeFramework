namespace CustomCodeFramework.Reports.Formats;

public static class ReportContentType
{
    public const string Pdf = "application/pdf";
    public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string Csv = "text/csv";
    public const string Word =
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    public const string Html = "text/html";
    public const string Json = "application/json";

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
