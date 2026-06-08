using CustomCodeFramework.Storage.Uploads;

namespace CustomCodeFramework.Storage.Validation;

public static class FileSizeValidator
{
    public static FileUploadValidationError? Validate(long? fileSizeInBytes, long maxSizeInBytes)
    {
        if (fileSizeInBytes is null)
        {
            return null;
        }

        if (fileSizeInBytes <= maxSizeInBytes)
        {
            return null;
        }

        return new FileUploadValidationError
        {
            Code = "storage.file_size_exceeded",
            Message = $"File size exceeds the maximum allowed size of {maxSizeInBytes} bytes.",
        };
    }
}
