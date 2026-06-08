namespace CustomCodeFramework.Reports.Results;

public abstract record ReportResult
{
    public Guid ResultId { get; init; } = Guid.NewGuid();

    public bool IsSuccess { get; init; }

    public bool IsFailure => !IsSuccess;

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
