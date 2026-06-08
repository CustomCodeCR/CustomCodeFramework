using System.Reflection;
using CustomCodeFramework.Reports.Definitions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CustomCodeFramework.Reports.Word.Sections;

public sealed class WordSectionBuilder
{
    public void Build(Body body, WordSection section, bool includeTableHeader)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(section);

        if (!string.IsNullOrWhiteSpace(section.Title))
        {
            body.Append(CreateParagraph(section.Title, "Heading2"));
        }

        foreach (var paragraph in section.Paragraphs)
        {
            body.Append(CreateParagraph(paragraph));
        }

        if (section.Rows.Count > 0)
        {
            body.Append(CreateTable(section.Rows, section.Columns, includeTableHeader));
        }
    }

    public static Paragraph CreateParagraph(string text, string? styleId = null)
    {
        var paragraphProperties = new ParagraphProperties();

        if (!string.IsNullOrWhiteSpace(styleId))
        {
            paragraphProperties.ParagraphStyleId = new ParagraphStyleId { Val = styleId };
        }

        return new Paragraph(
            paragraphProperties,
            new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })
        );
    }

    public static Paragraph CreateTitle(string text)
    {
        return CreateParagraph(text, "Title");
    }

    public static Paragraph CreateSubtitle(string text)
    {
        return CreateParagraph(text, "Subtitle");
    }

    private static Table CreateTable(
        IReadOnlyCollection<object> rows,
        IReadOnlyCollection<ReportColumnDefinition> columns,
        bool includeTableHeader
    )
    {
        var rowArray = rows.ToArray();
        var resolvedColumns = ResolveColumns(columns, rowArray[0]).ToArray();

        var table = new Table();

        table.AppendChild(
            new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                )
            )
        );

        if (includeTableHeader)
        {
            var headerRow = new TableRow();

            foreach (var column in resolvedColumns)
            {
                headerRow.Append(CreateCell(column.Header, bold: true));
            }

            table.Append(headerRow);
        }

        foreach (var row in rowArray)
        {
            var tableRow = new TableRow();

            foreach (var column in resolvedColumns)
            {
                var value = GetPropertyValue(row, column.Key);

                tableRow.Append(CreateCell(FormatValue(value, column.Format), bold: false));
            }

            table.Append(tableRow);
        }

        return table;
    }

    private static TableCell CreateCell(string? text, bool bold)
    {
        var runProperties = new RunProperties();

        if (bold)
        {
            runProperties.Append(new Bold());
        }

        return new TableCell(
            new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }),
            new Paragraph(
                new Run(
                    runProperties,
                    new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }
                )
            )
        );
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
            DateTime dateTime => dateTime.ToString(format),
            DateOnly dateOnly => dateOnly.ToString(format),
            TimeOnly timeOnly => timeOnly.ToString(format),
            decimal decimalValue => decimalValue.ToString(format),
            double doubleValue => doubleValue.ToString(format),
            float floatValue => floatValue.ToString(format),
            int intValue => intValue.ToString(format),
            long longValue => longValue.ToString(format),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private sealed record ColumnDescriptor(string Key, string Header, string? Format, int Order);
}
