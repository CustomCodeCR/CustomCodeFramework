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
}
