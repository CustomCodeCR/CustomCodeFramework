using CustomCodeFramework.Observability.Correlation;
using Serilog.Core;
using Serilog.Events;

namespace CustomCodeFramework.Observability.Logging;

public sealed class LogEnricher(ICorrelationIdAccessor correlationIdAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = correlationIdAccessor.CorrelationId;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("CorrelationId", correlationId)
            );
        }
    }
}
