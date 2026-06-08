namespace CustomCodeFramework.Reports.Pdf.Rendering;

public sealed class PdfRenderOptions
{
    public const string SectionName = "Reports:Pdf:Render";

    public string PaperSize { get; set; } = "A4";

    public double MarginTop { get; set; } = 10;
    public double MarginBottom { get; set; } = 10;
    public double MarginLeft { get; set; } = 10;
    public double MarginRight { get; set; } = 10;

    public int Dpi { get; set; } = 300;

    public bool EnableLocalFileAccess { get; set; }
}
