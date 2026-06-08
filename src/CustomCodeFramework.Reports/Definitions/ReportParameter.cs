namespace CustomCodeFramework.Reports.Definitions;

public sealed record ReportParameter
{
    public required string Name { get; init; }

    public required ReportParameterType Type { get; init; }

    public bool IsRequired { get; init; }

    public object? DefaultValue { get; init; }

    public IReadOnlyCollection<string> AllowedValues { get; init; } = [];

    public string? Description { get; init; }
}
