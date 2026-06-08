namespace CustomCodeFramework.Reports.Jobs;

public sealed record ReportJobResult
{
    public Guid JobId { get; init; }

    public ReportJobStatus Status { get; init; }

    public string? FileUrl { get; init; }

    public string? FilePath { get; init; }

    public string? ErrorMessage { get; init; }
}
