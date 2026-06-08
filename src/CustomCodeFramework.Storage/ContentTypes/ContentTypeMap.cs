using CustomCodeFramework.Storage.Abstractions;

namespace CustomCodeFramework.Storage.ContentTypes;

public sealed class ContentTypeMap : IContentTypeProvider
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ContentType.Pdf,
        [".csv"] = ContentType.Csv,
        [".json"] = ContentType.Json,
        [".txt"] = ContentType.Text,
        [".png"] = ContentType.Png,
        [".jpg"] = ContentType.Jpeg,
        [".jpeg"] = ContentType.Jpeg,
        [".gif"] = ContentType.Gif,
        [".webp"] = ContentType.Webp,
        [".xlsx"] = ContentType.Excel,
        [".docx"] = ContentType.Word,
        [".zip"] = ContentType.Zip,
    };

    public string GetContentType(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            return ContentType.Binary;
        }

        return Map.TryGetValue(extension, out var contentType) ? contentType : ContentType.Binary;
    }
}
