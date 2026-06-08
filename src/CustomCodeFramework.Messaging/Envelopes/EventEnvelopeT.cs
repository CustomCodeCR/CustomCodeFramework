using CustomCodeFramework.Messaging.Metadata;

namespace CustomCodeFramework.Messaging.Envelopes;

public sealed record EventEnvelope<TEvent>
{
    public Guid EventId { get; init; }

    public required string EventName { get; init; }

    public required TEvent Payload { get; init; }

    public EventMetadata Metadata { get; init; } = new();
}
