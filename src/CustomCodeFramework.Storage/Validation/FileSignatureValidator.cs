using CustomCodeFramework.Storage.Uploads;

namespace CustomCodeFramework.Storage.Validation;

public static class FileSignatureValidator
{
    private static readonly Dictionary<string, byte[][]> Signatures = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        [".pdf"] =
        [
            [0x25, 0x50, 0x44, 0x46],
        ],
        [".png"] =
        [
            [0x89, 0x50, 0x4E, 0x47],
        ],
        [".jpg"] =
        [
            [0xFF, 0xD8, 0xFF],
        ],
        [".jpeg"] =
        [
            [0xFF, 0xD8, 0xFF],
        ],
        [".zip"] =
        [
            [0x50, 0x4B, 0x03, 0x04],
            [0x50, 0x4B, 0x05, 0x06],
            [0x50, 0x4B, 0x07, 0x08],
        ],
        [".docx"] =
        [
            [0x50, 0x4B, 0x03, 0x04],
        ],
        [".xlsx"] =
        [
            [0x50, 0x4B, 0x03, 0x04],
        ],
    };

    public static async Task<FileUploadValidationError?> ValidateAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var extension = Path.GetExtension(fileName);

        if (!Signatures.TryGetValue(extension, out var signatures))
        {
            return null;
        }

        if (!stream.CanSeek)
        {
            return null;
        }

        var originalPosition = stream.Position;

        try
        {
            var maxSignatureLength = signatures.Max(signature => signature.Length);
            var buffer = new byte[maxSignatureLength];

            stream.Position = 0;

            var read = await stream.ReadAsync(
                buffer.AsMemory(0, maxSignatureLength),
                cancellationToken
            );

            var isValid = signatures.Any(signature =>
                read >= signature.Length && buffer.Take(signature.Length).SequenceEqual(signature)
            );

            if (isValid)
            {
                return null;
            }

            return new FileUploadValidationError
            {
                Code = "storage.invalid_file_signature",
                Message = $"File signature does not match extension '{extension}'.",
            };
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }
}
