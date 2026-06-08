using System.Diagnostics;

namespace CustomCodeFramework.Observability.Tracing;

public static class ActivitySources
{
    public const string DefaultSourceName = "CustomCodeFramework";

    public static readonly ActivitySource Default = new(DefaultSourceName);

    public static ActivitySource Create(string sourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        return new ActivitySource(sourceName);
    }
}
