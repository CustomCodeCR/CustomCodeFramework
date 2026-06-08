using CustomCodeFramework.Storage.Uploads;

namespace CustomCodeFramework.Storage.Validation;

public static class FileExtensionValidator
{
    public static FileUploadValidationError? Validate(
        string fileName,
        IReadOnlyCollection<string> allowedExtensions
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(allowedExtensions);

        if (allowedExtensions.Count == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(fileName);

        if (allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        return new FileUploadValidationError
        {
            Code = "storage.extension_not_allowed",
            Message = $"File extension '{extension}' is not allowed.",
        };
    }
}
