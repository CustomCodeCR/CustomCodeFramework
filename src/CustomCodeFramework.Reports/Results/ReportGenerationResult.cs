namespace CustomCodeFramework.Reports.Results;

public sealed record ReportGenerationResult : ReportResult
{
    public ReportFileResult? File { get; init; }

    public string? StoredFileUrl { get; init; }

    public string? StoredFilePath { get; init; }

    public static ReportGenerationResult Success(
        ReportFileResult file,
        string? storedFileUrl = null,
        string? storedFilePath = null
    )
    {
        return new ReportGenerationResult
        {
            IsSuccess = true,
            File = file,
            StoredFileUrl = storedFileUrl,
            StoredFilePath = storedFilePath,
        };
    }

    public static ReportGenerationResult Failure(string errorCode, string errorMessage)
    {
        return new ReportGenerationResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }
}
