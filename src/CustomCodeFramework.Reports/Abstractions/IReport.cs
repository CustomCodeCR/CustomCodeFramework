using CustomCodeFramework.Reports.Definitions;

namespace CustomCodeFramework.Reports.Abstractions;

public interface IReport
{
    string ReportKey { get; }

    ReportDefinition Definition { get; }
}
