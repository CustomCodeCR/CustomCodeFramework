using CustomCodeFramework.Reports.Formats;

namespace CustomCodeFramework.Reports.Templates;

public sealed record ReportTemplate
{
    public required string TemplateKey { get; init; }

    public required ReportFormat Format { get; init; }

    public required string Content { get; init; }

    public string? Language { get; init; }

    public bool IsHtml { get; init; }
}
