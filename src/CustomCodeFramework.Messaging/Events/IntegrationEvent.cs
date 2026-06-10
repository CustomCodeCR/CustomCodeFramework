namespace CustomCodeFramework.Messaging.Events;

public abstract record IntegrationEvent
{
    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid EventId { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public string? CorrelationId { get; init; }

    public string? SourceService { get; init; }

    public int Version { get; init; } = 1;
}
