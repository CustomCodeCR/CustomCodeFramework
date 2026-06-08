using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Options;

namespace CustomCodeFramework.Storage.Paths;

public sealed class StoragePathBuilder(StorageOptions options, IFileNameGenerator fileNameGenerator)
    : IFilePathBuilder
{
    public StoragePath BuildPath(string? folder, string originalFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);

        var fileName = fileNameGenerator.Generate(originalFileName);

        return new StoragePath
        {
            Bucket = options.DefaultBucket,
            Folder = string.IsNullOrWhiteSpace(folder) ? options.DefaultFolder : folder,
            FileName = fileName,
        };
    }
}
