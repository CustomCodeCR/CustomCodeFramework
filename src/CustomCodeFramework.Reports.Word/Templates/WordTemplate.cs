namespace CustomCodeFramework.Reports.Word.Templates;

public sealed record WordTemplate
{
    public required string TemplateKey { get; init; }

    public required string Title { get; init; }

    public string? Subtitle { get; init; }

    public IReadOnlyCollection<string> Paragraphs { get; init; } = [];

    public IReadOnlyDictionary<string, object?> Values { get; init; } =
        new Dictionary<string, object?>();
}
