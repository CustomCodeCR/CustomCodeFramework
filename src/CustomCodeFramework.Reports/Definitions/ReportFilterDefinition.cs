namespace CustomCodeFramework.Reports.Definitions;

public sealed record ReportFilterDefinition
{
    public required string Key { get; init; }

    public required string Label { get; init; }

    public required ReportParameterType Type { get; init; }

    public bool IsRequired { get; init; }

    public object? DefaultValue { get; init; }
}
