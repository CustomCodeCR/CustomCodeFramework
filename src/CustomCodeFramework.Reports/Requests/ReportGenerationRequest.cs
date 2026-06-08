using CustomCodeFramework.Reports.Formats;

namespace CustomCodeFramework.Reports.Requests;

public sealed record ReportGenerationRequest : ReportRequest
{
    public ReportFormat Format { get; init; } = ReportFormat.Pdf;

    public string? TemplateKey { get; init; }

    public bool StoreResult { get; init; }
}
