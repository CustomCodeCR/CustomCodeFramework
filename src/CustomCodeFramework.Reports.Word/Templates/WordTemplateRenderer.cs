namespace CustomCodeFramework.Reports.Word.Templates;

public sealed class WordTemplateRenderer
{
    public WordTemplate Render(WordTemplate template, IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);

        return template with
        {
            Title = RenderText(template.Title, values),
            Subtitle = RenderText(template.Subtitle, values),
            Paragraphs = template
                .Paragraphs.Select(paragraph => RenderText(paragraph, values) ?? string.Empty)
                .ToArray(),
            Values = values,
        };
    }

    private static string? RenderText(string? text, IReadOnlyDictionary<string, object?> values)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var rendered = text;

        foreach (var value in values)
        {
            rendered = rendered.Replace(
                "{{" + value.Key + "}}",
                value.Value?.ToString(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        return rendered;
    }
}
