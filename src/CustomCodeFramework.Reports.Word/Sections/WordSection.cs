using CustomCodeFramework.Reports.Definitions;

namespace CustomCodeFramework.Reports.Word.Sections;

public sealed record WordSection
{
    public string? Title { get; init; }

    public IReadOnlyCollection<string> Paragraphs { get; init; } = [];

    public IReadOnlyCollection<object> Rows { get; init; } = [];

    public IReadOnlyCollection<ReportColumnDefinition> Columns { get; init; } = [];
}
