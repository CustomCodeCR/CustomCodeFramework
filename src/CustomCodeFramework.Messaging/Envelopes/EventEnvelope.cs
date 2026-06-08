using CustomCodeFramework.Messaging.Metadata;

namespace CustomCodeFramework.Messaging.Envelopes;

public sealed record EventEnvelope
{
    public Guid EventId { get; init; }

    public required string EventName { get; init; }

    public required string EventType { get; init; }

    public required string Payload { get; init; }

    public EventMetadata Metadata { get; init; } = new();
}
