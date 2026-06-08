namespace CustomCodeFramework.Storage.ContentTypes;

public static class ContentTypeValidator
{
    public static bool IsAllowed(
        string contentType,
        IReadOnlyCollection<string> allowedContentTypes
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(allowedContentTypes);

        if (allowedContentTypes.Count == 0)
        {
            return true;
        }

        return allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }
}
