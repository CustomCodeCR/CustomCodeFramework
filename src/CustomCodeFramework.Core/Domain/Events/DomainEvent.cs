namespace CustomCodeFramework.Core.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid EventId { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}
