using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;

namespace CustomCodeFramework.Reports.Templates;

public sealed class InMemoryReportTemplateProvider : IReportTemplateProvider
{
    private readonly Dictionary<string, ReportTemplate> _templates = [];

    public Task<ReportTemplate?> GetTemplateAsync(
        string templateKey,
        ReportFormat format,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);

        var key = BuildKey(templateKey, format);

        _templates.TryGetValue(key, out var template);

        return Task.FromResult(template);
    }

    public void AddTemplate(ReportTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        _templates[BuildKey(template.TemplateKey, template.Format)] = template;
    }

    private static string BuildKey(string templateKey, ReportFormat format)
    {
        return $"{templateKey}:{format}".ToLowerInvariant();
    }
}
