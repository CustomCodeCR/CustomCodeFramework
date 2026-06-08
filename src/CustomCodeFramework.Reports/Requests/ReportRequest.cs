namespace CustomCodeFramework.Reports.Requests;

public abstract record ReportRequest
{
    public Guid RequestId { get; init; } = Guid.NewGuid();

    public required string ReportKey { get; init; }

    public IReadOnlyDictionary<string, object?> Parameters { get; init; } =
        new Dictionary<string, object?>();

    public string? RequestedBy { get; init; }

    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;
}
