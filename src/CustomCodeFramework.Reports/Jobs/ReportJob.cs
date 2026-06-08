using CustomCodeFramework.Reports.Formats;

namespace CustomCodeFramework.Reports.Jobs;

public sealed record ReportJob
{
    public Guid JobId { get; init; } = Guid.NewGuid();

    public required string ReportKey { get; init; }

    public ReportFormat Format { get; init; }

    public ReportJobStatus Status { get; init; } = ReportJobStatus.Pending;

    public IReadOnlyDictionary<string, object?> Parameters { get; init; } =
        new Dictionary<string, object?>();

    public string? RequestedBy { get; init; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public string? ErrorMessage { get; init; }
}
