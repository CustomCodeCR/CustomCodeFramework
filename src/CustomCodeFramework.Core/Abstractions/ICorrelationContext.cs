namespace CustomCodeFramework.Core.Abstractions;

public interface ICorrelationContext
{
    string? CorrelationId { get; }
}
