namespace CustomCodeFramework.Storage.Abstractions;

public interface IFileNameGenerator
{
    string Generate(string originalFileName);
}
