using CustomCodeFramework.Messaging.Events;

namespace CustomCodeFramework.Messaging.Abstractions;

public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
