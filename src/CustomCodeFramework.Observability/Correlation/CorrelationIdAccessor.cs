namespace CustomCodeFramework.Observability.Correlation;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; set; }
}

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

    public string? CorrelationId
    {
        get => CurrentCorrelationId.Value;
        set => CurrentCorrelationId.Value = value;
    }
}
