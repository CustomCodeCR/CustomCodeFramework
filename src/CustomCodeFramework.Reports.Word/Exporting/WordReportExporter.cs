using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;
using CustomCodeFramework.Reports.Results;
using CustomCodeFramework.Reports.Word.Sections;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Word.Exporting;

public sealed class WordReportExporter(
    WordSectionBuilder sectionBuilder,
    IOptions<WordExportOptions> options
) : IReportExporter
{
    public ReportFormat Format => ReportFormat.Word;

    public Task<ReportFileResult> ExportAsync(
        ReportExportRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using var stream = new MemoryStream();

            using (
                var document = WordprocessingDocument.Create(
                    stream,
                    DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
                    autoSave: true
                )
            )
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();

                var body = new Body();

                body.Append(WordSectionBuilder.CreateTitle(ResolveTitle(request)));

                if (options.Value.IncludeGeneratedAt)
                {
                    body.Append(
                        WordSectionBuilder.CreateParagraph(
                            $"Generated at UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        )
                    );
                }

                if (options.Value.IncludeParameters && request.Parameters.Count > 0)
                {
                    body.Append(WordSectionBuilder.CreateParagraph("Parameters", "Heading2"));

                    foreach (var parameter in request.Parameters)
                    {
                        body.Append(
                            WordSectionBuilder.CreateParagraph(
                                $"{parameter.Key}: {parameter.Value}"
                            )
                        );
                    }
                }

                sectionBuilder.Build(
                    body,
                    new WordSection
                    {
                        Title = "Data",
                        Rows = request.Rows,
                        Columns = request.Columns,
                    },
                    options.Value.IncludeTableHeader
                );

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            var content = stream.ToArray();

            return Task.FromResult(
                ReportFileResult.Success(
                    ResolveFileName(request),
                    ReportContentType.Word,
                    content,
                    ReportFormat.Word
                )
            );
        }
        catch (Exception exception)
        {
            return Task.FromResult(
                ReportFileResult.Failure("word.export_failed", exception.Message)
            );
        }
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
            ? $"{request.ReportKey}-{DateTime.UtcNow:yyyyMMddHHmmss}.docx"
            : request.FileName;

        if (!fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".docx";
        }

        return fileName;
    }
}
