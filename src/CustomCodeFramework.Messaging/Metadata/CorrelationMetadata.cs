namespace CustomCodeFramework.Messaging.Metadata;

public sealed record CorrelationMetadata
{
    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public string? TraceId { get; init; }

    public static CorrelationMetadata Empty => new();
}
