namespace CustomCodeFramework.Messaging.Metadata;

public sealed record EventMetadata
{
    public string EventName { get; init; } = default!;

    public string EventType { get; init; } = default!;

    public string ContentType { get; init; } = "application/json";

    public string? Source { get; init; }

    public string? Version { get; init; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public CorrelationMetadata Correlation { get; init; } = CorrelationMetadata.Empty;

    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>();
}
