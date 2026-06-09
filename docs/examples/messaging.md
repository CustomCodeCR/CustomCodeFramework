# CustomCodeFramework.Messaging

## Introducción

`CustomCodeFramework.Messaging` es el paquete base para manejar eventos de integración, publicación de eventos, consumo de eventos, envelopes, metadata, serialización, outbox e inbox.

Este paquete no está amarrado a un broker específico.

No debe saber si se usa:

- RabbitMQ.
- Kafka.
- Azure Service Bus.
- AWS SQS.
- Redis Streams.
- PostgreSQL Outbox.
- HTTP Webhooks.

Su responsabilidad es definir las abstracciones y modelos comunes para que cualquier proveedor pueda implementarlos.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Messaging --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Messaging": {
    "Outbox": {
      "Enabled": true,
      "BatchSize": 50,
      "MaxRetryCount": 5,
      "PollingIntervalSeconds": 10
    },
    "Inbox": {
      "Enabled": true,
      "ProcessedMessageExpirationDays": 30
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Messaging.DependencyInjection;

builder.Services.AddCustomCodeMessaging(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Messaging
├── Abstractions
│   ├── IEventBus.cs
│   ├── IEventPublisher.cs
│   ├── IEventConsumer.cs
│   └── IIntegrationEventHandler.cs
│
├── Events
│   ├── IntegrationEvent.cs
│   └── IntegrationEventNameAttribute.cs
│
├── Envelopes
│   ├── EventEnvelope.cs
│   └── EventEnvelopeT.cs
│
├── Metadata
│   ├── EventMetadata.cs
│   ├── MessageHeaders.cs
│   └── CorrelationMetadata.cs
│
├── Serialization
│   ├── IMessageSerializer.cs
│   └── SystemTextJsonMessageSerializer.cs
│
├── Outbox
│   ├── IOutboxStore.cs
│   └── OutboxOptions.cs
│
├── Inbox
│   ├── IInboxStore.cs
│   └── InboxOptions.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# ¿Qué problema resuelve?

Sin una capa de messaging, los servicios terminan publicando mensajes directamente al broker:

```csharp
await rabbitMq.PublishAsync("client-created", payload);
```

Problemas:

```text
Application queda acoplado a RabbitMQ
difícil cambiar a Kafka/Azure Service Bus
no hay metadata estándar
no hay correlation id
no hay outbox
no hay inbox
no hay idempotencia
cada servicio serializa diferente
```

Con `CustomCodeFramework.Messaging`:

```text
Application
    ↓
IEventPublisher
    ↓
IOutboxStore / IEventBus
    ↓
Broker concreto
```

---

# Conceptos principales

## Integration Event

Un evento de integración representa algo que ya ocurrió y debe comunicarse a otros sistemas o servicios.

Ejemplos:

```text
ClientCreatedIntegrationEvent
SalesOrderApprovedIntegrationEvent
InvoiceIssuedIntegrationEvent
PaymentReceivedIntegrationEvent
ReportGeneratedIntegrationEvent
NotificationSentIntegrationEvent
```

Los eventos deben nombrarse en pasado:

```text
Created
Updated
Approved
Rejected
Cancelled
Issued
Generated
Sent
Processed
```

---

# IntegrationEvent

Clase base para eventos de integración.

## Ejemplo

```csharp
using CustomCodeFramework.Messaging.Events;

public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

# IntegrationEventNameAttribute

Permite asignar un nombre externo al evento.

Esto evita depender del nombre de la clase como nombre del mensaje.

## Ejemplo

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

Nombre interno:

```text
ClientCreatedIntegrationEvent
```

Nombre externo:

```text
client-created
```

---

# Reglas de naming para eventos

## Sí

```csharp
[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(...) : IntegrationEvent;
```

```csharp
[IntegrationEventName("sales-order-approved")]
public sealed record SalesOrderApprovedIntegrationEvent(...) : IntegrationEvent;
```

```csharp
[IntegrationEventName("invoice-issued")]
public sealed record InvoiceIssuedIntegrationEvent(...) : IntegrationEvent;
```

## No

```csharp
[IntegrationEventName("create-client")]
public sealed record CreateClientIntegrationEvent(...) : IntegrationEvent;
```

```csharp
[IntegrationEventName("client-create")]
public sealed record ClientCreateIntegrationEvent(...) : IntegrationEvent;
```

Los eventos representan hechos pasados, no comandos.

---

# EventEnvelope

## ¿Qué es?

Un envelope envuelve el evento con metadata.

El evento solo contiene datos de negocio.

El envelope contiene información técnica:

```text
message id
event name
event type
occurred at
correlation id
causation id
tenant id
user id
headers
payload
```

---

## Ejemplo conceptual

```json
{
  "messageId": "4f1c046e-8e2d-4d83-9034-6a2f6eeb3342",
  "eventName": "client-created",
  "eventType": "ClientCreatedIntegrationEvent",
  "occurredAtUtc": "2026-06-09T15:30:00Z",
  "metadata": {
    "correlationId": "corr-123",
    "causationId": "cmd-456",
    "userId": "user-789"
  },
  "payload": {
    "clientId": "0f4b8a3d-8a9f-4c15-9d2d-78c0c6c8f5c0",
    "name": "ACME",
    "email": "client@example.com"
  }
}
```

---

# EventEnvelope<T>

Envelope tipado.

## Ejemplo

```csharp
using CustomCodeFramework.Messaging.Envelopes;

var envelope = new EventEnvelope<ClientCreatedIntegrationEvent>
{
    MessageId = Guid.NewGuid(),
    EventName = "client-created",
    Event = new ClientCreatedIntegrationEvent(
        clientId,
        "ACME",
        "client@example.com"),
    Metadata = new EventMetadata
    {
        CorrelationId = "corr-123",
        UserId = "user-123"
    },
    OccurredAtUtc = DateTime.UtcNow
};
```

---

# EventMetadata

## ¿Para qué sirve?

Contiene metadata técnica del evento.

Campos típicos:

```text
CorrelationId
CausationId
UserId
TenantId
Source
TraceId
OccurredAtUtc
```

---

## Ejemplo

```csharp
using CustomCodeFramework.Messaging.Metadata;

var metadata = new EventMetadata
{
    CorrelationId = "corr-123",
    CausationId = "cmd-456",
    UserId = "user-789",
    TenantId = "tenant-001",
    Source = "clients-api"
};
```

---

# CorrelationMetadata

## ¿Para qué sirve?

Permite rastrear una operación entre múltiples servicios.

Ejemplo de flujo:

```text
HTTP Request
    ↓ correlation id: corr-123
Clients.Api
    ↓ event: client-created, correlation id: corr-123
Notifications.Api
    ↓ sends email, correlation id: corr-123
Audit.Api
    ↓ stores audit, correlation id: corr-123
```

---

# MessageHeaders

## ¿Para qué sirve?

Define nombres estándar de headers.

Ejemplos:

```text
x-correlation-id
x-causation-id
x-message-id
x-event-name
x-event-type
x-tenant-id
x-user-id
```

---

# IMessageSerializer

## ¿Qué es?

Abstracción para serializar y deserializar mensajes.

Permite cambiar la implementación sin modificar la aplicación.

---

## Uso

```csharp
using CustomCodeFramework.Messaging.Serialization;

public sealed class MessageSerializationExample(
    IMessageSerializer serializer)
{
    public byte[] Serialize(ClientCreatedIntegrationEvent integrationEvent)
    {
        return serializer.Serialize(integrationEvent);
    }

    public ClientCreatedIntegrationEvent? Deserialize(byte[] payload)
    {
        return serializer.Deserialize<ClientCreatedIntegrationEvent>(payload);
    }
}
```

---

# SystemTextJsonMessageSerializer

Implementación por defecto usando `System.Text.Json`.

Recomendaciones:

```text
usar records
usar propiedades públicas
evitar ciclos
no serializar entidades EF
serializar DTOs/eventos simples
```

Correcto:

```csharp
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

Incorrecto:

```csharp
public sealed record ClientCreatedIntegrationEvent(
    Client Client) : IntegrationEvent;
```

---

# IEventPublisher

## ¿Qué es?

Abstracción para publicar eventos.

La aplicación debe depender de `IEventPublisher`, no del broker concreto.

---

## Uso básico

```csharp
using CustomCodeFramework.Messaging.Abstractions;

public sealed class ClientEventPublisher(IEventPublisher publisher)
{
    public Task PublishAsync(
        Guid clientId,
        string name,
        string email,
        CancellationToken cancellationToken)
    {
        return publisher.PublishAsync(
            new ClientCreatedIntegrationEvent(
                clientId,
                name,
                email),
            cancellationToken);
    }
}
```

---

# IEventBus

## ¿Qué es?

Representa el bus de eventos.

Puede encargarse de publicar y suscribir eventos.

Uso conceptual:

```csharp
using CustomCodeFramework.Messaging.Abstractions;

public sealed class EventBusExample(IEventBus eventBus)
{
    public Task PublishAsync(
        ClientCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        return eventBus.PublishAsync(
            integrationEvent,
            cancellationToken);
    }
}
```

---

# IEventConsumer

## ¿Qué es?

Representa un consumidor de eventos.

Lo implementan providers concretos como RabbitMQ, Kafka o Azure Service Bus.

Uso conceptual:

```csharp
using CustomCodeFramework.Messaging.Abstractions;

public sealed class ClientCreatedConsumer(IEventConsumer consumer)
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return consumer.StartAsync(cancellationToken);
    }
}
```

---

# IIntegrationEventHandler<TEvent>

## ¿Qué es?

Handler encargado de procesar un evento de integración.

---

## Ejemplo

```csharp
using CustomCodeFramework.Messaging.Abstractions;

public sealed class ClientCreatedIntegrationEventHandler
    : IIntegrationEventHandler<ClientCreatedIntegrationEvent>
{
    public Task HandleAsync(
        ClientCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Client created: {integrationEvent.ClientId}");

        return Task.CompletedTask;
    }
}
```

---

# Ejemplo real: Cliente creado

## Evento

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

## Publicador

```csharp
using CustomCodeFramework.Messaging.Abstractions;

public sealed class ClientCreatedPublisher(
    IEventPublisher publisher)
{
    public Task PublishAsync(
        Guid clientId,
        string name,
        string email,
        CancellationToken cancellationToken)
    {
        return publisher.PublishAsync(
            new ClientCreatedIntegrationEvent(
                clientId,
                name,
                email),
            cancellationToken);
    }
}
```

---

## Handler consumidor

```csharp
using CustomCodeFramework.Messaging.Abstractions;

public sealed class ClientCreatedIntegrationEventHandler
    : IIntegrationEventHandler<ClientCreatedIntegrationEvent>
{
    public Task HandleAsync(
        ClientCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"New client: {integrationEvent.Name}");

        return Task.CompletedTask;
    }
}
```

---

# Ejemplo real: Enviar notificación al crear cliente

## Evento

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class SendWelcomeEmailWhenClientCreatedHandler(
    INotificationSender notificationSender)
    : IIntegrationEventHandler<ClientCreatedIntegrationEvent>
{
    public Task HandleAsync(
        ClientCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Email = integrationEvent.Email,
                    Name = integrationEvent.Name
                },
                Subject = "Welcome",
                Body = $"Welcome {integrationEvent.Name}."
            },
            cancellationToken);
    }
}
```

---

# Ejemplo real: Crear audit log al recibir evento

## Evento

```csharp
[IntegrationEventName("invoice-issued")]
public sealed record InvoiceIssuedIntegrationEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid ClientId,
    decimal Total,
    DateTime IssuedAtUtc) : IntegrationEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Mongo.Repositories;

public sealed class CreateAuditLogWhenInvoiceIssuedHandler(
    MongoRepository<AuditLogReadModel> repository)
    : IIntegrationEventHandler<InvoiceIssuedIntegrationEvent>
{
    public Task HandleAsync(
        InvoiceIssuedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var auditId = Guid.NewGuid();

        return repository.InsertAsync(
            new AuditLogReadModel
            {
                Id = $"audit_log:{auditId}",
                AuditLogId = auditId,
                Action = "invoice-issued",
                EntityId = integrationEvent.InvoiceId.ToString(),
                Description = $"Invoice {integrationEvent.InvoiceNumber} was issued.",
                CreatedAtUtc = integrationEvent.IssuedAtUtc
            },
            cancellationToken);
    }
}
```

---

# Outbox Pattern

## ¿Qué problema resuelve?

Problema sin outbox:

```text
1. Guardar cliente en base de datos
2. Publicar evento client-created
```

Puede fallar así:

```text
Base de datos OK
Evento falla
```

O:

```text
Evento OK
Base de datos falla
```

Esto causa inconsistencias.

---

# Solución con Outbox

Guardar la entidad y el mensaje en la misma transacción.

```text
CommandHandler
    ↓
Guardar entidad
    ↓
Guardar outbox_message
    ↓
Commit
    ↓
Outbox worker publica evento
```

---

# Flujo Outbox

```text
ClientCreatedDomainEvent
    ↓
OutboxSaveChangesInterceptor
    ↓
outbox_messages table
    ↓
OutboxProcessor
    ↓
IEventPublisher / IEventBus
    ↓
Broker
```

---

# IOutboxStore

## ¿Qué es?

Contrato para guardar y leer mensajes pendientes de publicación.

---

## Uso conceptual

```csharp
using CustomCodeFramework.Messaging.Outbox;

public sealed class OutboxExample(IOutboxStore outboxStore)
{
    public Task AddAsync(
        ClientCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        return outboxStore.AddAsync(
            integrationEvent,
            cancellationToken);
    }
}
```

---

# OutboxOptions

Configuración:

```json
{
  "Messaging": {
    "Outbox": {
      "Enabled": true,
      "BatchSize": 50,
      "MaxRetryCount": 5,
      "PollingIntervalSeconds": 10
    }
  }
}
```

---

# Ejemplo Outbox Message

```json
{
  "outboxMessageId": "65b436ea-6b49-49c7-b915-68de442589d1",
  "eventName": "client-created",
  "eventType": "ClientCreatedIntegrationEvent",
  "payload": "{...}",
  "occurredAtUtc": "2026-06-09T15:30:00Z",
  "processedAtUtc": null,
  "retryCount": 0,
  "error": null
}
```

---

# Worker conceptual de Outbox

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Messaging.Outbox;

public sealed class OutboxWorker(
    IOutboxStore outboxStore,
    IEventPublisher publisher)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var messages = await outboxStore.GetPendingAsync(
            batchSize: 50,
            cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(
                    message,
                    cancellationToken);

                await outboxStore.MarkAsProcessedAsync(
                    message.MessageId,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                await outboxStore.MarkAsFailedAsync(
                    message.MessageId,
                    exception.Message,
                    cancellationToken);
            }
        }
    }
}
```

---

# Inbox Pattern

## ¿Qué problema resuelve?

Cuando un sistema recibe mensajes, puede recibir el mismo mensaje más de una vez.

Causas:

```text
broker reintenta
consumer falla después de procesar
timeout
redelivery
at-least-once delivery
```

Sin inbox, el evento podría procesarse dos veces.

---

# Ejemplo del problema

Evento:

```text
payment-received
```

Sin inbox:

```text
Primera entrega -> crea recibo
Segunda entrega -> crea recibo duplicado
```

---

# Solución con Inbox

Guardar los mensajes procesados.

```text
Mensaje recibido
    ↓
Revisar inbox
    ↓
Si ya existe -> ignorar
    ↓
Si no existe -> procesar
    ↓
Guardar inbox
```

---

# IInboxStore

Contrato para verificar y guardar mensajes procesados.

---

## Uso conceptual

```csharp
using CustomCodeFramework.Messaging.Inbox;

public sealed class InboxExample(IInboxStore inboxStore)
{
    public async Task HandleAsync(
        Guid messageId,
        Func<Task> handler,
        CancellationToken cancellationToken)
    {
        var alreadyProcessed = await inboxStore.ExistsAsync(
            messageId,
            cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        await handler();

        await inboxStore.MarkAsProcessedAsync(
            messageId,
            cancellationToken);
    }
}
```

---

# InboxOptions

Configuración:

```json
{
  "Messaging": {
    "Inbox": {
      "Enabled": true,
      "ProcessedMessageExpirationDays": 30
    }
  }
}
```

---

# Ejemplo Inbox Message

```json
{
  "inboxMessageId": "65b436ea-6b49-49c7-b915-68de442589d1",
  "eventName": "payment-received",
  "processedAtUtc": "2026-06-09T15:35:00Z",
  "consumer": "billing-service"
}
```

---

# Ejemplo real: PaymentReceived idempotente

## Evento

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("payment-received")]
public sealed record PaymentReceivedIntegrationEvent(
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount,
    DateTime ReceivedAtUtc) : IntegrationEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class PaymentReceivedIntegrationEventHandler(
    IInboxStore inboxStore,
    IRepository<Invoice, Guid> invoiceRepository,
    IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<PaymentReceivedIntegrationEvent>
{
    public async Task HandleAsync(
        PaymentReceivedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await inboxStore.ExistsAsync(
            integrationEvent.EventId,
            cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        var invoice = await invoiceRepository.GetByIdAsync(
            integrationEvent.InvoiceId,
            cancellationToken);

        if (invoice is null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        invoice.RegisterPayment(
            integrationEvent.PaymentId,
            integrationEvent.Amount,
            integrationEvent.ReceivedAtUtc);

        invoiceRepository.Update(invoice);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await inboxStore.MarkAsProcessedAsync(
            integrationEvent.EventId,
            cancellationToken);
    }
}
```

---

# Publicación desde un CommandHandler

## Command

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed record ApproveSalesOrderCommand(
    Guid SalesOrderId) : ICommand;
```

---

## Integration Event

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("sales-order-approved")]
public sealed record SalesOrderApprovedIntegrationEvent(
    Guid SalesOrderId,
    Guid ClientId,
    decimal Total,
    DateTime ApprovedAtUtc) : IntegrationEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class ApproveSalesOrderCommandHandler(
    IRepository<SalesOrder, Guid> repository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<ApproveSalesOrderCommand>
{
    public async Task HandleAsync(
        ApproveSalesOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var salesOrder = await repository.GetByIdAsync(
            command.SalesOrderId,
            cancellationToken);

        if (salesOrder is null)
        {
            throw new InvalidOperationException("Sales order not found.");
        }

        salesOrder.Approve();

        repository.Update(salesOrder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new SalesOrderApprovedIntegrationEvent(
                salesOrder.Id,
                salesOrder.ClientId,
                salesOrder.Total,
                DateTime.UtcNow),
            cancellationToken);
    }
}
```

Nota:

```text
Si usás Outbox, preferiblemente el evento se guarda en outbox y no se publica directamente al broker dentro del handler.
```

---

# Domain Event vs Integration Event

## Domain Event

Evento interno del dominio.

Ejemplo:

```csharp
public sealed record ClientCreatedDomainEvent(
    Guid ClientId) : DomainEvent;
```

Se usa dentro del mismo bounded context.

---

## Integration Event

Evento externo para otros servicios.

Ejemplo:

```csharp
[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

Se usa para comunicar entre servicios.

---

# Conversión DomainEvent -> IntegrationEvent

## Domain Event

```csharp
public sealed record ClientCreatedDomainEvent(
    Guid ClientId,
    string Name,
    string Email) : DomainEvent;
```

---

## Integration Event

```csharp
[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

## Mapper

```csharp
public static class ClientIntegrationEventMapper
{
    public static ClientCreatedIntegrationEvent ToIntegrationEvent(
        ClientCreatedDomainEvent domainEvent)
    {
        return new ClientCreatedIntegrationEvent(
            domainEvent.ClientId,
            domainEvent.Name,
            domainEvent.Email);
    }
}
```

---

# Event Versioning

Los eventos publicados no deben romper consumidores existentes.

## Versión 1

```csharp
[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

## Versión 2 segura

Agregar campo opcional o nuevo evento.

```csharp
[IntegrationEventName("client-created-v2")]
public sealed record ClientCreatedV2IntegrationEvent(
    Guid ClientId,
    string Name,
    string Email,
    string? PhoneNumber) : IntegrationEvent;
```

---

# Reglas para versionado

## Sí

```text
crear evento v2 si el cambio rompe contrato
mantener v1 mientras haya consumidores
agregar campos opcionales
documentar payload
```

## No

```text
renombrar propiedades existentes
cambiar tipos existentes
eliminar campos usados
cambiar significado de un campo
```

---

# Event Payload Design

## Correcto

```csharp
public sealed record InvoiceIssuedIntegrationEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid ClientId,
    decimal Total,
    string Currency,
    DateTime IssuedAtUtc) : IntegrationEvent;
```

## Incorrecto

```csharp
public sealed record InvoiceIssuedIntegrationEvent(
    Invoice Invoice) : IntegrationEvent;
```

Razones:

```text
serializa demasiado
expone dominio interno
puede tener ciclos
rompe consumidores al cambiar la entidad
```

---

# Ejemplo: Evento de factura emitida

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("invoice-issued")]
public sealed record InvoiceIssuedIntegrationEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid ClientId,
    string ClientName,
    decimal Subtotal,
    decimal Taxes,
    decimal Total,
    string Currency,
    DateTime IssuedAtUtc) : IntegrationEvent;
```

---

# Handler: Crear notificación

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class SendInvoiceEmailWhenIssuedHandler(
    INotificationSender notificationSender)
    : IIntegrationEventHandler<InvoiceIssuedIntegrationEvent>
{
    public Task HandleAsync(
        InvoiceIssuedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Name = integrationEvent.ClientName
                },
                Subject = $"Invoice {integrationEvent.InvoiceNumber}",
                Body = $"Your invoice total is {integrationEvent.Total} {integrationEvent.Currency}."
            },
            cancellationToken);
    }
}
```

---

# Handler: Crear read model en Mongo

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Mongo.Repositories;

public sealed class CreateInvoiceReadModelWhenIssuedHandler(
    MongoRepository<InvoiceReadModel> repository)
    : IIntegrationEventHandler<InvoiceIssuedIntegrationEvent>
{
    public Task HandleAsync(
        InvoiceIssuedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return repository.InsertAsync(
            new InvoiceReadModel
            {
                Id = $"invoice:{integrationEvent.InvoiceId}",
                InvoiceId = integrationEvent.InvoiceId,
                InvoiceNumber = integrationEvent.InvoiceNumber,
                ClientId = integrationEvent.ClientId,
                ClientName = integrationEvent.ClientName,
                Subtotal = integrationEvent.Subtotal,
                Taxes = integrationEvent.Taxes,
                Total = integrationEvent.Total,
                Currency = integrationEvent.Currency,
                IssuedAtUtc = integrationEvent.IssuedAtUtc
            },
            cancellationToken);
    }
}
```

---

# Organización recomendada

```text
Application
├── Clients
│   ├── CreateClient
│   │   ├── CreateClientCommand.cs
│   │   ├── CreateClientCommandHandler.cs
│   │   └── ClientCreatedIntegrationEvent.cs
│   │
│   └── Events
│       ├── ClientCreatedIntegrationEvent.cs
│       └── ClientUpdatedIntegrationEvent.cs
│
Infrastructure
├── Messaging
│   ├── Handlers
│   │   ├── SendWelcomeEmailWhenClientCreatedHandler.cs
│   │   └── CreateAuditLogWhenInvoiceIssuedHandler.cs
│   │
│   ├── Outbox
│   │   └── OutboxWorker.cs
│   │
│   └── Inbox
│       └── InboxProcessor.cs
```

Otra opción por feature:

```text
Features
└── Clients
    ├── Events
    │   ├── ClientCreatedIntegrationEvent.cs
    │   └── ClientUpdatedIntegrationEvent.cs
    │
    ├── Handlers
    │   ├── SendWelcomeEmailWhenClientCreatedHandler.cs
    │   └── UpdateClientProjectionWhenClientUpdatedHandler.cs
    │
    └── CreateClient
        ├── CreateClientCommand.cs
        └── CreateClientCommandHandler.cs
```

---

# Integración con CQRS

## Command publica evento

```text
CreateClientCommand
    ↓
CreateClientCommandHandler
    ↓
IEventPublisher
    ↓
ClientCreatedIntegrationEvent
```

---

# Integración con Persistence

## Outbox

```text
CommandHandler
    ↓
Repository
    ↓
UnitOfWork
    ↓
OutboxStore
    ↓
OutboxProcessor
    ↓
EventBus
```

---

# Integración con Mongo

## Projections

```text
IntegrationEvent
    ↓
IIntegrationEventHandler
    ↓
MongoRepository<ReadModel>
```

---

# Integración con Notifications

## Notificaciones por eventos

```text
ClientCreatedIntegrationEvent
    ↓
SendWelcomeEmailHandler
    ↓
INotificationSender
```

---

# Integración con Redis

## Lock del worker

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class OutboxWorker(
    IDistributedLock distributedLock,
    IOutboxProcessor outboxProcessor)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var handle = await distributedLock.AcquireAsync(
            "locks:outbox-worker",
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (handle is null)
        {
            return;
        }

        await outboxProcessor.ProcessAsync(cancellationToken);
    }
}
```

---

# Program.cs

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Messaging.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Messaging API",
    version: "v1");

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();

builder.Services.AddCustomCodeMessaging(builder.Configuration);

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "Messaging.Api",
    serviceVersion: "0.1.0");

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapControllers();

app.Run();
```

---

# Ejemplo de broker adapter

Este paquete no trae RabbitMQ/Kafka directamente, pero un provider concreto podría implementar `IEventPublisher`.

## Adapter conceptual

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Messaging.Serialization;

public sealed class RabbitMqEventPublisher(
    IMessageSerializer serializer)
    : IEventPublisher
{
    public Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var payload = serializer.Serialize(integrationEvent);

        // Publicar payload en RabbitMQ.

        return Task.CompletedTask;
    }
}
```

---

# Buenas prácticas

## Sí

Eventos en pasado:

```csharp
ClientCreatedIntegrationEvent
```

Payload pequeño:

```csharp
Guid ClientId
string Name
string Email
```

Usar metadata:

```text
CorrelationId
CausationId
UserId
TenantId
```

Usar outbox para publicación confiable.

Usar inbox para idempotencia.

Versionar eventos si cambia el contrato.

---

## No

No publicar entidades completas:

```csharp
Client Client
```

No usar eventos como comandos:

```csharp
CreateClientIntegrationEvent
```

No publicar al broker antes de confirmar base de datos.

No procesar mensajes sin idempotencia.

No cambiar contratos publicados sin versión.

---

# Errores comunes

## Evento con nombre incorrecto

Incorrecto:

```csharp
public sealed record CreateClientIntegrationEvent(...) : IntegrationEvent;
```

Correcto:

```csharp
public sealed record ClientCreatedIntegrationEvent(...) : IntegrationEvent;
```

---

## Publicar entidades completas

Incorrecto:

```csharp
public sealed record ClientCreatedIntegrationEvent(Client Client) : IntegrationEvent;
```

Correcto:

```csharp
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

## No usar Outbox

Incorrecto:

```csharp
await repository.AddAsync(client);
await unitOfWork.SaveChangesAsync();
await eventPublisher.PublishAsync(new ClientCreatedIntegrationEvent(...));
```

Puede fallar después del commit.

Mejor:

```text
guardar evento en outbox en la misma transacción
```

---

## No usar Inbox

Incorrecto:

```csharp
public async Task HandleAsync(PaymentReceivedIntegrationEvent e)
{
    invoice.RegisterPayment(e.PaymentId, e.Amount);
    await unitOfWork.SaveChangesAsync();
}
```

Si llega dos veces, procesa dos veces.

Correcto:

```csharp
if (await inboxStore.ExistsAsync(e.EventId, cancellationToken))
{
    return;
}
```

---

# Flujo recomendado de publicación

```text
Command
    ↓
Aggregate
    ↓
DomainEvent
    ↓
Outbox
    ↓
OutboxWorker
    ↓
EventBus
    ↓
Broker
```

---

# Flujo recomendado de consumo

```text
Broker
    ↓
Consumer
    ↓
Inbox check
    ↓
IntegrationEventHandler
    ↓
Save changes
    ↓
Mark inbox processed
```

---

# Resumen

`CustomCodeFramework.Messaging` debe usarse para:

```text
definir eventos de integración
publicar eventos sin acoplarse al broker
consumir eventos con handlers
serializar mensajes
transportar metadata
usar outbox
usar inbox
mantener idempotencia
mantener trazabilidad
```

No es un broker. Es la base común para que cualquier broker pueda integrarse de forma limpia.

Combinación recomendada:

```text
CQRS -> produce eventos
Persistence -> guarda outbox/inbox
Messaging -> define contratos y publicación
Redis -> lock de workers
Mongo -> proyecciones/read models
Notifications -> reacciones a eventos
```
