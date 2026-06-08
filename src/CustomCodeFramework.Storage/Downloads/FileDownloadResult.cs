using CustomCodeFramework.Storage.Files;

namespace CustomCodeFramework.Storage.Downloads;

public sealed record FileDownloadResult
{
    public bool IsSuccess { get; init; }

    public bool IsFailure => !IsSuccess;

    public FileObject? File { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static FileDownloadResult Success(FileObject file)
    {
        return new FileDownloadResult { IsSuccess = true, File = file };
    }

    public static FileDownloadResult Failure(string code, string message)
    {
        return new FileDownloadResult
        {
            IsSuccess = false,
            ErrorCode = code,
            ErrorMessage = message,
        };
    }
}
