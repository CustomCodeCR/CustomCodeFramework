using CustomCodeFramework.Storage.Downloads;
using CustomCodeFramework.Storage.Uploads;

namespace CustomCodeFramework.Storage.Abstractions;

public interface IFileStorage
{
    Task<FileUploadResult> UploadAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default
    );

    Task<FileDownloadResult> DownloadAsync(
        FileDownloadRequest request,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
