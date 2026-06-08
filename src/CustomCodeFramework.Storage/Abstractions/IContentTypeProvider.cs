namespace CustomCodeFramework.Storage.Abstractions;

public interface IContentTypeProvider
{
    string GetContentType(string fileName);
}
