namespace CustomCodeFramework.Reports.Definitions;

public sealed record ReportColumnDefinition
{
    public required string Key { get; init; }

    public required string Header { get; init; }

    public int Order { get; init; }

    public string? Format { get; init; }

    public string? Width { get; init; }

    public bool IsVisible { get; init; } = true;
}
