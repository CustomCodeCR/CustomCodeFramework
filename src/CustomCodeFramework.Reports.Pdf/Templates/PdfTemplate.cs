namespace CustomCodeFramework.Reports.Pdf.Templates;

public sealed record PdfTemplate
{
    public required string TemplateKey { get; init; }

    public required string Html { get; init; }

    public IReadOnlyDictionary<string, object?> Values { get; init; } =
        new Dictionary<string, object?>();
}
