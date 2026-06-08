using CustomCodeFramework.Storage.Paths;

namespace CustomCodeFramework.Storage.Abstractions;

public interface IFilePathBuilder
{
    StoragePath BuildPath(string? folder, string originalFileName);
}
