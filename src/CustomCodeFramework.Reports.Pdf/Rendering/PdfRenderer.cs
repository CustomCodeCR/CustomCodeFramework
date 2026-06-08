using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Reports.Pdf.Rendering;

public sealed class PdfRenderer(IConverter converter, IOptions<PdfRenderOptions> options)
{
    public byte[] Render(string html, bool landscape = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(html);

        var renderOptions = options.Value;

        var document = new HtmlToPdfDocument
        {
            GlobalSettings =
            {
                PaperSize = ResolvePaperKind(renderOptions.PaperSize),
                Orientation = landscape ? Orientation.Landscape : Orientation.Portrait,
                Margins = new MarginSettings
                {
                    Top = renderOptions.MarginTop,
                    Bottom = renderOptions.MarginBottom,
                    Left = renderOptions.MarginLeft,
                    Right = renderOptions.MarginRight,
                },
                DPI = renderOptions.Dpi,
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings =
                    {
                        DefaultEncoding = "utf-8",
                        LoadImages = true,
                        EnableIntelligentShrinking = true,
                    },
                    LoadSettings = { BlockLocalFileAccess = !renderOptions.EnableLocalFileAccess },
                },
            },
        };

        return converter.Convert(document);
    }

    private static PaperKind ResolvePaperKind(string paperSize)
    {
        if (Enum.TryParse<PaperKind>(paperSize, ignoreCase: true, out var paperKind))
        {
            return paperKind;
        }

        return PaperKind.A4;
    }
}
