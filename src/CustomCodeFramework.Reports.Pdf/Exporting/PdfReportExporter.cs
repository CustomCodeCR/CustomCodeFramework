using System.Globalization;
using System.Reflection;
using System.Text;
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Definitions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Pdf.Rendering;
using CustomCodeFramework.Reports.Pdf.Templates;
using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Pdf.Exporting;

public sealed class PdfReportExporter(
    PdfRenderer renderer,
    PdfTemplateRenderer templateRenderer,
    IOptions<PdfExportOptions> options
) : IReportExporter
{
    public ReportFormat Format => ReportFormat.Pdf;

    public Task<ReportFileResult> ExportAsync(
        ReportExportRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var html = BuildHtml(request);

            var content = renderer.Render(html, options.Value.UseLandscape);

            return Task.FromResult(
                ReportFileResult.Success(
                    ResolveFileName(request),
                    ReportContentType.Pdf,
                    content,
                    ReportFormat.Pdf
                )
            );
        }
        catch (Exception exception)
        {
            return Task.FromResult(
                ReportFileResult.Failure("pdf.export_failed", exception.Message)
            );
        }
    }

    private string BuildHtml(ReportExportRequest request)
    {
        var title = ResolveTitle(request);

        var body = new StringBuilder();

        body.AppendLine($"<h1>{PdfTemplateRenderer.EscapeHtml(title)}</h1>");

        if (options.Value.IncludeGeneratedAt)
        {
            body.AppendLine(
                $"<p class=\"meta\">Generated at UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</p>"
            );
        }

        if (options.Value.IncludeParameters && request.Parameters.Count > 0)
        {
            body.AppendLine("<h2>Parameters</h2>");

            foreach (var parameter in request.Parameters)
            {
                body.AppendLine(
                    $"<p><strong>{PdfTemplateRenderer.EscapeHtml(parameter.Key)}:</strong> {PdfTemplateRenderer.EscapeHtml(parameter.Value?.ToString())}</p>"
                );
            }
        }

        if (request.Rows.Count > 0)
        {
            body.AppendLine("<h2>Data</h2>");
            body.AppendLine(BuildTable(request.Rows, request.Columns));
        }

        return templateRenderer.RenderHtmlDocument(title, body.ToString());
    }

    private string BuildTable(
        IReadOnlyCollection<object> rows,
        IReadOnlyCollection<ReportColumnDefinition> columns
    )
    {
        var rowArray = rows.ToArray();

        if (rowArray.Length == 0)
        {
            return string.Empty;
        }

        var resolvedColumns = ResolveColumns(columns, rowArray[0]).ToArray();

        var html = new StringBuilder();

        html.AppendLine("<table>");

        if (options.Value.IncludeTableHeader)
        {
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");

            foreach (var column in resolvedColumns)
            {
                html.AppendLine($"<th>{PdfTemplateRenderer.EscapeHtml(column.Header)}</th>");
            }

            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
        }

        html.AppendLine("<tbody>");

        foreach (var row in rowArray)
        {
            html.AppendLine("<tr>");

            foreach (var column in resolvedColumns)
            {
                var value = GetPropertyValue(row, column.Key);
                var formattedValue = FormatValue(value, column.Format);

                html.AppendLine($"<td>{PdfTemplateRenderer.EscapeHtml(formattedValue)}</td>");
            }

            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");

        return html.ToString();
    }

    private static IReadOnlyCollection<ColumnDescriptor> ResolveColumns(
        IReadOnlyCollection<ReportColumnDefinition> columns,
        object sampleRow
    )
    {
        if (columns.Count > 0)
        {
            return columns
                .Where(column => column.IsVisible)
                .OrderBy(column => column.Order)
                .Select(column => new ColumnDescriptor(
                    column.Key,
                    column.Header,
                    column.Format,
                    column.Order
                ))
                .ToArray();
        }

        return sampleRow
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(
                (property, index) => new ColumnDescriptor(property.Name, property.Name, null, index)
            )
            .OrderBy(column => column.Order)
            .ToArray();
    }

    private static object? GetPropertyValue(object row, string propertyName)
    {
        if (row is IDictionary<string, object?> dictionary)
        {
            return dictionary.TryGetValue(propertyName, out var value) ? value : null;
        }

        var property = row.GetType()
            .GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
            );

        return property?.GetValue(row);
    }

    private static string FormatValue(object? value, string? format)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            return value.ToString() ?? string.Empty;
        }

        return value switch
        {
            DateTime dateTime => dateTime.ToString(format, CultureInfo.InvariantCulture),
            DateOnly dateOnly => dateOnly.ToString(format, CultureInfo.InvariantCulture),
            TimeOnly timeOnly => timeOnly.ToString(format, CultureInfo.InvariantCulture),
            decimal decimalValue => decimalValue.ToString(format, CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString(format, CultureInfo.InvariantCulture),
            float floatValue => floatValue.ToString(format, CultureInfo.InvariantCulture),
            int intValue => intValue.ToString(format, CultureInfo.InvariantCulture),
            long longValue => longValue.ToString(format, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private string ResolveTitle(ReportExportRequest request)
    {
        if (
            request.Parameters.TryGetValue("title", out var title)
            && title is not null
            && !string.IsNullOrWhiteSpace(title.ToString())
        )
        {
            return title.ToString()!;
        }

        return string.IsNullOrWhiteSpace(options.Value.DefaultTitle)
            ? request.ReportKey
            : options.Value.DefaultTitle;
    }

    private static string ResolveFileName(ReportExportRequest request)
    {
        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? $"{request.ReportKey}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
            : request.FileName;

        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".pdf";
        }

        return fileName;
    }

    private sealed record ColumnDescriptor(string Key, string Header, string? Format, int Order);
}
