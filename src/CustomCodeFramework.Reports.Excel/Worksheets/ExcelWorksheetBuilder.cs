using System.Reflection;
using ClosedXML.Excel;
using CustomCodeFramework.Reports.Excel.Exporting;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Excel.Worksheets;

public sealed class ExcelWorksheetBuilder(IOptions<ExcelExportOptions> options)
{
    public void BuildWorksheet(XLWorkbook workbook, ExcelWorksheetDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(workbook);
        ArgumentNullException.ThrowIfNull(definition);

        var worksheetName = string.IsNullOrWhiteSpace(definition.Name)
            ? options.Value.DefaultWorksheetName
            : definition.Name;

        var worksheet = workbook.Worksheets.Add(worksheetName);
        var rows = definition.Rows.ToArray();

        if (rows.Length == 0)
        {
            return;
        }

        var columns = ResolveColumns(definition, rows[0]).ToArray();

        var currentRow = 1;

        if (options.Value.IncludeHeader)
        {
            WriteHeader(worksheet, currentRow, columns, definition);

            currentRow++;
        }

        WriteRows(worksheet, currentRow, rows, columns, definition);

        var lastRow = currentRow + rows.Length - 1;
        var lastColumn = columns.Length;

        if (options.Value.UseTable && lastRow >= 1 && lastColumn >= 1)
        {
            var range = worksheet.Range(1, 1, lastRow, lastColumn);
            range.CreateTable();
        }

        ApplyColumnStyles(worksheet, columns, definition);

        if (options.Value.FreezeHeaderRow && options.Value.IncludeHeader)
        {
            worksheet.SheetView.FreezeRows(1);
        }

        if (options.Value.AutoAdjustColumns)
        {
            worksheet.Columns().AdjustToContents();
        }
    }

    private static void WriteHeader(
        IXLWorksheet worksheet,
        int rowNumber,
        IReadOnlyList<ColumnDescriptor> columns,
        ExcelWorksheetDefinition definition
    )
    {
        for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var cell = worksheet.Cell(rowNumber, columnIndex + 1);
            cell.Value = columns[columnIndex].Header;

            if (definition.StyleOptions.BoldHeader)
            {
                cell.Style.Font.Bold = true;
            }

            if (definition.StyleOptions.CenterHeader)
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            if (definition.StyleOptions.ApplyBorders)
            {
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }
    }

    private static void WriteRows(
        IXLWorksheet worksheet,
        int startRow,
        IReadOnlyList<object> rows,
        IReadOnlyList<ColumnDescriptor> columns,
        ExcelWorksheetDefinition definition
    )
    {
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                var value = GetPropertyValue(row, column.Key);
                var cell = worksheet.Cell(startRow + rowIndex, columnIndex + 1);

                SetCellValue(cell, value);

                if (!string.IsNullOrWhiteSpace(column.Format))
                {
                    cell.Style.NumberFormat.Format = column.Format;
                }

                if (definition.StyleOptions.ApplyBorders)
                {
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                if (definition.StyleOptions.WrapText)
                {
                    cell.Style.Alignment.WrapText = true;
                }
            }
        }
    }

    private static void ApplyColumnStyles(
        IXLWorksheet worksheet,
        IReadOnlyList<ColumnDescriptor> columns,
        ExcelWorksheetDefinition definition
    )
    {
        foreach (var style in definition.ColumnStyles)
        {
            var columnIndex = columns
                .Select((column, index) => new { column, index })
                .FirstOrDefault(current =>
                    current.column.Key.Equals(style.ColumnKey, StringComparison.OrdinalIgnoreCase)
                )
                ?.index;

            if (columnIndex is null)
            {
                continue;
            }

            var excelColumn = worksheet.Column(columnIndex.Value + 1);

            if (!string.IsNullOrWhiteSpace(style.NumberFormat))
            {
                excelColumn.Style.NumberFormat.Format = style.NumberFormat;
            }

            if (style.Width is not null)
            {
                excelColumn.Width = style.Width.Value;
            }

            if (style.IsBold)
            {
                excelColumn.Style.Font.Bold = true;
            }

            if (style.WrapText)
            {
                excelColumn.Style.Alignment.WrapText = true;
            }
        }
    }

    private static IReadOnlyCollection<ColumnDescriptor> ResolveColumns(
        ExcelWorksheetDefinition definition,
        object sampleRow
    )
    {
        if (definition.Columns.Count > 0)
        {
            return definition
                .Columns.Where(column => column.IsVisible)
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

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Clear();
                break;

            case string stringValue:
                cell.Value = stringValue;
                break;

            case int intValue:
                cell.Value = intValue;
                break;

            case long longValue:
                cell.Value = longValue;
                break;

            case decimal decimalValue:
                cell.Value = decimalValue;
                break;

            case double doubleValue:
                cell.Value = doubleValue;
                break;

            case float floatValue:
                cell.Value = floatValue;
                break;

            case bool boolValue:
                cell.Value = boolValue;
                break;

            case DateTime dateTime:
                cell.Value = dateTime;
                break;

            case DateOnly dateOnly:
                cell.Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                break;

            case TimeOnly timeOnly:
                cell.Value = timeOnly.ToTimeSpan();
                break;

            case Guid guid:
                cell.Value = guid.ToString();
                break;

            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private sealed record ColumnDescriptor(string Key, string Header, string? Format, int Order);
}
