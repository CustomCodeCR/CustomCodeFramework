using CustomCodeFramework.Messaging.Events;

namespace Company.Service.Contracts.Events;

public sealed record ServiceCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid EntityId { get; init; }
}
