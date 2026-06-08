namespace CustomCodeFramework.Storage.Uploads;

public sealed record FileUploadValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public IReadOnlyCollection<FileUploadValidationError> Errors { get; init; } = [];

    public static FileUploadValidationResult Success()
    {
        return new FileUploadValidationResult();
    }

    public static FileUploadValidationResult Failure(
        IReadOnlyCollection<FileUploadValidationError> errors
    )
    {
        return new FileUploadValidationResult { Errors = errors };
    }
}

public sealed record FileUploadValidationError
{
    public required string Code { get; init; }

    public required string Message { get; init; }
}
