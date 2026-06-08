namespace CustomCodeFramework.Reports.Templates;

public sealed record ReportTemplateOptions
{
    public string? TemplateKey { get; init; }

    public string? Language { get; init; }

    public bool UseDefaultTemplate { get; init; } = true;
}
