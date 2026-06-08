namespace CustomCodeFramework.Messaging.Inbox;

public interface IInboxStore
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task AddAsync(
        Guid eventId,
        string eventName,
        DateTime processedAtUtc,
        CancellationToken cancellationToken = default
    );
}
