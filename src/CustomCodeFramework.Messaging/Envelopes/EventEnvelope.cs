using CustomCodeFramework.Messaging.Metadata;

namespace CustomCodeFramework.Messaging.Envelopes;

public sealed record EventEnvelope
{
    public Guid EventId { get; init; }

    public required string EventName { get; init; }

    public required string EventType { get; init; }

    public required string Payload { get; init; }

    public string? CorrelationId { get; init; }

    public string? SourceService { get; init; }

    public int Version { get; init; } = 1;

    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    public EventMetadata Metadata { get; init; } = new();
}
