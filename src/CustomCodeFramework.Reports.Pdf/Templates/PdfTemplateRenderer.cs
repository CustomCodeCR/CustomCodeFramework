namespace CustomCodeFramework.Reports.Pdf.Templates;

public sealed class PdfTemplateRenderer
{
    public string Render(PdfTemplate template, IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);

        var html = template.Html;

        foreach (var value in values)
        {
            html = html.Replace(
                "{{" + value.Key + "}}",
                value.Value?.ToString(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        return html;
    }

    public string RenderHtmlDocument(string title, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <title>{{EscapeHtml(title)}}</title>
                <style>
                    * {
                        box-sizing: border-box;
                    }

                    body {
                        font-family: Arial, Helvetica, sans-serif;
                        font-size: 12px;
                        color: #111827;
                        margin: 0;
                        padding: 0;
                    }

                    h1 {
                        font-size: 22px;
                        margin: 0 0 12px 0;
                    }

                    h2 {
                        font-size: 16px;
                        margin: 18px 0 8px 0;
                    }

                    p {
                        margin: 4px 0;
                    }

                    .meta {
                        color: #6b7280;
                        font-size: 10px;
                        margin-bottom: 12px;
                    }

                    table {
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 10px;
                    }

                    th,
                    td {
                        border: 1px solid #d1d5db;
                        padding: 6px;
                        text-align: left;
                        vertical-align: top;
                    }

                    th {
                        background: #f3f4f6;
                        font-weight: bold;
                    }
                </style>
            </head>
            <body>
                {{body}}
            </body>
            </html>
            """;
    }

    public static string EscapeHtml(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&#39;", StringComparison.Ordinal);
    }
}
