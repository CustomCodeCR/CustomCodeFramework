using CustomCodeFramework.Messaging.Events;
using CustomCodeFramework.Messaging.Metadata;

namespace CustomCodeFramework.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        EventMetadata? metadata = null,
        CancellationToken cancellationToken = default
    )
        where TEvent : IntegrationEvent;
}
