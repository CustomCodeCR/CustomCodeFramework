using CustomCodeFramework.Messaging.Events;

namespace CustomCodeFramework.Messaging.Abstractions;

public interface IEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    void Subscribe<TEvent>()
        where TEvent : IntegrationEvent;
}
