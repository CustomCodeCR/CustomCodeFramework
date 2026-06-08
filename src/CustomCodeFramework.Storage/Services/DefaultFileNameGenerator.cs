using CustomCodeFramework.Storage.Abstractions;

namespace CustomCodeFramework.Storage.Services;

public sealed class DefaultFileNameGenerator : IFileNameGenerator
{
    public string Generate(string originalFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);

        var extension = Path.GetExtension(originalFileName);
        var safeName = Path.GetFileNameWithoutExtension(originalFileName)
            .Trim()
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        safeName = string.Concat(
            safeName.Select(character =>
                char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-'
            )
        );

        return $"{safeName}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
    }
}
