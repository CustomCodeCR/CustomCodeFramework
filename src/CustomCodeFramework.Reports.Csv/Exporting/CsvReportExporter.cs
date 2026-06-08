using System.Globalization;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Csv.Formatting;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Csv.Exporting;

public sealed class CsvReportExporter(IOptions<CsvExportOptions> options) : IReportExporter
{
    public ReportFormat Format => ReportFormat.Csv;

    public async Task<ReportFileResult> ExportAsync(
        ReportExportRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var csvOptions = options.Value;

            await using var stream = new MemoryStream();
            await using var writer = CreateWriter(stream, csvOptions);

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ResolveDelimiter(csvOptions.Delimiter),
                HasHeaderRecord = csvOptions.IncludeHeader,
            };

            await using var csvWriter = new CsvWriter(writer, configuration);

            var rows = request.Rows.ToArray();

            if (rows.Length == 0)
            {
                await writer.FlushAsync(cancellationToken);

                return BuildResult(request, stream.ToArray());
            }

            var columns = ResolveColumns(request, rows[0]);

            if (csvOptions.IncludeHeader)
            {
                foreach (var column in columns)
                {
                    csvWriter.WriteField(column.Header);
                }

                await csvWriter.NextRecordAsync();
            }

            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var column in columns)
                {
                    var value = GetPropertyValue(row, column.Key);
                    var formattedValue = CsvColumnFormatter.FormatValue(value, column.Format);

                    csvWriter.WriteField(formattedValue);
                }

                await csvWriter.NextRecordAsync();
            }

            await writer.FlushAsync(cancellationToken);

            return BuildResult(request, stream.ToArray());
        }
        catch (Exception exception)
        {
            return ReportFileResult.Failure("csv.export_failed", exception.Message);
        }
    }

    private static StreamWriter CreateWriter(Stream stream, CsvExportOptions options)
    {
        var encoding = options.UseUtf8Bom
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        return new StreamWriter(stream, encoding, bufferSize: 1024, leaveOpen: true);
    }

    private static IReadOnlyCollection<ColumnDescriptor> ResolveColumns(
        ReportExportRequest request,
        object sampleRow
    )
    {
        if (request.Columns.Count > 0)
        {
            return request
                .Columns.Where(column => column.IsVisible)
                .OrderBy(column => column.Order)
                .Select(column => new ColumnDescriptor(column.Key, column.Header, column.Format))
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
            return dictionary.TryGetValue(propertyName, out var dictionaryValue)
                ? dictionaryValue
                : null;
        }

        var property = row.GetType()
            .GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
            );

        return property?.GetValue(row);
    }

    private static string ResolveDelimiter(CsvDelimiter delimiter)
    {
        return delimiter switch
        {
            CsvDelimiter.Comma => ",",
            CsvDelimiter.Semicolon => ";",
            CsvDelimiter.Tab => "\t",
            CsvDelimiter.Pipe => "|",
            _ => ",",
        };
    }

    private static ReportFileResult BuildResult(ReportExportRequest request, byte[] content)
    {
        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? $"{request.ReportKey}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
            : request.FileName;

        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".csv";
        }

        return ReportFileResult.Success(fileName, ReportContentType.Csv, content, ReportFormat.Csv);
    }

    private sealed record ColumnDescriptor(
        string Key,
        string Header,
        string? Format,
        int Order = 0
    );
}
