namespace CustomCodeFramework.Reports.Definitions;

public sealed record ReportDefinition
{
    public required string ReportKey { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public IReadOnlyCollection<ReportParameter> Parameters { get; init; } = [];

    public IReadOnlyCollection<ReportColumnDefinition> Columns { get; init; } = [];

    public IReadOnlyCollection<ReportFilterDefinition> Filters { get; init; } = [];

    public bool IsEnabled { get; init; } = true;
}
