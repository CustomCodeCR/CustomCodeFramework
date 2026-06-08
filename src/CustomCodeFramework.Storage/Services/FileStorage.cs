using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.ContentTypes;
using CustomCodeFramework.Storage.Downloads;
using CustomCodeFramework.Storage.Files;
using CustomCodeFramework.Storage.Options;
using CustomCodeFramework.Storage.Uploads;
using CustomCodeFramework.Storage.Validation;

namespace CustomCodeFramework.Storage.Services;

public sealed class FileStorage(
    IFileStorageProvider provider,
    IFilePathBuilder pathBuilder,
    IContentTypeProvider contentTypeProvider,
    StorageOptions options
) : IFileStorage
{
    public async Task<FileUploadResult> UploadAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return FileUploadResult.Failure(validationResult.Errors);
        }

        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? contentTypeProvider.GetContentType(request.FileName)
            : request.ContentType;

        var path = pathBuilder.BuildPath(request.Options.Folder, request.FileName);

        var fileObject = new FileObject
        {
            FileName = path.FileName,
            Path = path.FullPath,
            Content = new FileContent
            {
                Stream = request.Content,
                ContentType = contentType,
                Length = request.SizeInBytes,
            },
            Metadata = new FileMetadata
            {
                OriginalFileName = request.FileName,
                ContentType = contentType,
                SizeInBytes = request.SizeInBytes ?? 0,
                UploadedBy = request.UploadedBy,
                Properties = request.Options.Metadata,
            },
        };

        var storedFile = await provider.SaveAsync(
            fileObject,
            request.Options.Visibility,
            cancellationToken
        );

        return FileUploadResult.Success(storedFile);
    }

    public async Task<FileDownloadResult> DownloadAsync(
        FileDownloadRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var file = await provider.GetAsync(request.Path, cancellationToken);

        if (file is null)
        {
            return FileDownloadResult.Failure(
                "storage.file_not_found",
                $"File '{request.Path}' was not found."
            );
        }

        return FileDownloadResult.Success(file);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return provider.ExistsAsync(path, cancellationToken);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return provider.DeleteAsync(path, cancellationToken);
    }

    private async Task<FileUploadValidationResult> ValidateAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken
    )
    {
        var errors = new List<FileUploadValidationError>();

        var maxSize = request.Options.MaxSizeInBytes ?? options.MaxFileSizeInBytes;

        var sizeError = FileSizeValidator.Validate(request.SizeInBytes, maxSize);

        if (sizeError is not null)
        {
            errors.Add(sizeError);
        }

        var allowedExtensions =
            request.Options.AllowedExtensions.Count > 0 ? request.Options.AllowedExtensions : [];

        var extensionError = FileExtensionValidator.Validate(request.FileName, allowedExtensions);

        if (extensionError is not null)
        {
            errors.Add(extensionError);
        }

        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? contentTypeProvider.GetContentType(request.FileName)
            : request.ContentType;

        var allowedContentTypes = request.Options.AllowedContentTypes;

        if (!ContentTypeValidator.IsAllowed(contentType, allowedContentTypes))
        {
            errors.Add(
                new FileUploadValidationError
                {
                    Code = "storage.content_type_not_allowed",
                    Message = $"Content type '{contentType}' is not allowed.",
                }
            );
        }

        if (options.ValidateFileSignature)
        {
            var signatureError = await FileSignatureValidator.ValidateAsync(
                request.Content,
                request.FileName,
                cancellationToken
            );

            if (signatureError is not null)
            {
                errors.Add(signatureError);
            }
        }

        return errors.Count == 0
            ? FileUploadValidationResult.Success()
            : FileUploadValidationResult.Failure(errors);
    }
}
