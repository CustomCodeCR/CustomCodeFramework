using CustomCodeFramework.Reports.Formats;

namespace CustomCodeFramework.Reports.Results;

public sealed record ReportFileResult : ReportResult
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }

    public ReportFormat Format { get; init; }

    public long SizeInBytes => Content.LongLength;

    public static ReportFileResult Success(
        string fileName,
        string contentType,
        byte[] content,
        ReportFormat format
    )
    {
        return new ReportFileResult
        {
            IsSuccess = true,
            FileName = fileName,
            ContentType = contentType,
            Content = content,
            Format = format,
        };
    }

    public static ReportFileResult Failure(string errorCode, string errorMessage)
    {
        return new ReportFileResult
        {
            IsSuccess = false,
            FileName = string.Empty,
            ContentType = string.Empty,
            Content = [],
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }
}
