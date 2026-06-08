using CustomCodeFramework.Storage.Files;

namespace CustomCodeFramework.Storage.Uploads;

public sealed record FileUploadResult
{
    public bool IsSuccess { get; init; }

    public bool IsFailure => !IsSuccess;

    public StoredFile? File { get; init; }

    public IReadOnlyCollection<FileUploadValidationError> Errors { get; init; } = [];

    public static FileUploadResult Success(StoredFile file)
    {
        return new FileUploadResult { IsSuccess = true, File = file };
    }

    public static FileUploadResult Failure(IReadOnlyCollection<FileUploadValidationError> errors)
    {
        return new FileUploadResult { IsSuccess = false, Errors = errors };
    }

    public static FileUploadResult Failure(string code, string message)
    {
        return Failure([new FileUploadValidationError { Code = code, Message = message }]);
    }
}
