namespace CustomCodeFramework.Reports.Results;

public sealed record ReportValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public IReadOnlyCollection<ReportValidationError> Errors { get; init; } = [];

    public static ReportValidationResult Success()
    {
        return new ReportValidationResult();
    }

    public static ReportValidationResult Failure(IReadOnlyCollection<ReportValidationError> errors)
    {
        return new ReportValidationResult { Errors = errors };
    }
}

public sealed record ReportValidationError
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public string? ParameterName { get; init; }
}
