namespace CustomCodeFramework.Reports.Templates;

public sealed record ReportTemplateContext
{
    public IReadOnlyDictionary<string, object?> Values { get; init; } =
        new Dictionary<string, object?>();

    public object? Model { get; init; }

    public IReadOnlyCollection<object> Rows { get; init; } = [];
}
