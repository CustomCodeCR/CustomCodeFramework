using CustomCodeFramework.Core.Abstractions;

namespace CustomCodeFramework.Observability.Correlation;

public sealed class CorrelationContext(ICorrelationIdAccessor correlationIdAccessor)
    : ICorrelationContext
{
    public string? CorrelationId => correlationIdAccessor.CorrelationId;
}
