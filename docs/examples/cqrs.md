# CustomCodeFramework.Cqrs

## Introducción

`CustomCodeFramework.Cqrs` es el paquete encargado de separar las operaciones de escritura y lectura usando el patrón CQRS.

CQRS significa:

```text
Command Query Responsibility Segregation
```

La idea es separar claramente:

```text
Commands -> modifican estado
Queries  -> consultan datos
```

---

# ¿Qué problema resuelve?

Sin CQRS, normalmente los servicios terminan mezclando:

```text
validación
reglas de negocio
transacciones
logging
consultas
comandos
persistencia
eventos
```

Ejemplo problemático:

```csharp
public async Task<Guid> CreateClientAsync(CreateClientRequest request)
{
    Validate(request);

    logger.LogInformation("Creating client");

    using var transaction = await db.BeginTransactionAsync();

    var client = new Client(...);

    db.Clients.Add(client);

    await db.SaveChangesAsync();

    await eventBus.PublishAsync(...);

    await transaction.CommitAsync();

    return client.Id;
}
```

Con CQRS, eso se separa así:

```text
Endpoint
  ↓
CommandDispatcher
  ↓
Behaviors
  ↓
CommandHandler
  ↓
Repository / UnitOfWork
```

---

# Responsabilidades del paquete

`CustomCodeFramework.Cqrs` contiene:

- Commands.
- Queries.
- Command handlers.
- Query handlers.
- Command dispatcher.
- Query dispatcher.
- Pipeline behaviors.
- Logging behavior.
- Validation behavior.
- Transaction behavior.
- Unhandled exception behavior.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Cqrs --version 0.1.0-alpha.1
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Cqrs.DependencyInjection;

builder.Services.AddCustomCodeCqrs();
```

---

# Estructura interna

```text
CustomCodeFramework.Cqrs
├── Commands
│   ├── ICommand.cs
│   ├── ICommandT.cs
│   ├── ICommandHandler.cs
│   └── ICommandHandlerT.cs
│
├── Queries
│   ├── IQuery.cs
│   └── IQueryHandler.cs
│
├── Behaviors
│   ├── LoggingBehavior.cs
│   ├── ValidationBehavior.cs
│   ├── TransactionBehavior.cs
│   └── UnhandledExceptionBehavior.cs
│
├── Dispatching
│   ├── ICommandDispatcher.cs
│   └── IQueryDispatcher.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# Conceptos principales

## Command

Un command representa una intención de modificar el sistema.

Ejemplos:

```text
CreateClientCommand
UpdateClientCommand
DeleteClientCommand
ApproveOrderCommand
GenerateInvoiceCommand
```

Un command debe ser:

- explícito
- orientado a acción
- inmutable
- pequeño
- sin lógica de infraestructura

---

## Query

Una query representa una intención de consultar datos.

Ejemplos:

```text
GetClientByIdQuery
SearchClientsQuery
GetSalesReportQuery
GetAvailableStockQuery
```

Una query debe:

- no modificar estado
- devolver datos
- ser rápida
- poder usar read models
- poder usar Dapper, Mongo o cache

---

## Handler

Un handler ejecuta un command o query.

Debe contener la orquestación del caso de uso.

---

## Dispatcher

El dispatcher recibe un command/query y localiza su handler correspondiente.

---

## Behavior

Un behavior es una capa transversal del pipeline.

Ejemplos:

```text
LoggingBehavior
ValidationBehavior
TransactionBehavior
UnhandledExceptionBehavior
```

Sirve para no repetir código en cada handler.

---

# Commands sin retorno

## Definición

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed record DeleteClientCommand(Guid ClientId) : ICommand;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed class DeleteClientCommandHandler
    : ICommandHandler<DeleteClientCommand>
{
    public async Task HandleAsync(
        DeleteClientCommand command,
        CancellationToken cancellationToken = default)
    {
        // Buscar cliente.
        // Validar existencia.
        // Marcar como eliminado.
        // Guardar cambios.
    }
}
```

---

## Uso desde endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapDelete("/clients/{clientId:guid}", async (
    Guid clientId,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    await commandDispatcher.DispatchAsync(
        new DeleteClientCommand(clientId),
        cancellationToken);

    return Results.NoContent();
});
```

---

# Commands con retorno

## Definición

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed record CreateClientCommand(
    string Name,
    string Email) : ICommand<Guid>;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed class CreateClientCommandHandler
    : ICommandHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var clientId = Guid.NewGuid();

        // Crear entidad.
        // Guardar en base de datos.

        return clientId;
    }
}
```

---

## Uso desde endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapPost("/clients", async (
    CreateClientRequest request,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    var clientId = await commandDispatcher.DispatchAsync(
        new CreateClientCommand(
            request.Name,
            request.Email),
        cancellationToken);

    return Results.Created(
        $"/clients/{clientId}",
        new
        {
            ClientId = clientId
        });
});
```

---

# Queries

## Definición

```csharp
using CustomCodeFramework.Cqrs.Queries;

public sealed record GetClientByIdQuery(Guid ClientId)
    : IQuery<ClientResponse?>;
```

---

## Response DTO

```csharp
public sealed record ClientResponse(
    Guid ClientId,
    string Name,
    string Email,
    bool IsActive);
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Queries;

public sealed class GetClientByIdQueryHandler
    : IQueryHandler<GetClientByIdQuery, ClientResponse?>
{
    public async Task<ClientResponse?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        // Consultar base de datos o read model.

        return new ClientResponse(
            query.ClientId,
            "ACME",
            "client@example.com",
            true);
    }
}
```

---

## Uso desde endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapGet("/clients/{clientId:guid}", async (
    Guid clientId,
    IQueryDispatcher queryDispatcher,
    CancellationToken cancellationToken) =>
{
    var client = await queryDispatcher.DispatchAsync(
        new GetClientByIdQuery(clientId),
        cancellationToken);

    return client is null
        ? Results.NotFound()
        : Results.Ok(client);
});
```

---

# Ejemplo real completo: Crear cliente

## Request HTTP

```csharp
public sealed record CreateClientRequest(
    string Name,
    string Email,
    string PhoneNumber);
```

---

## Command

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed record CreateClientCommand(
    string Name,
    string Email,
    string PhoneNumber) : ICommand<Guid>;
```

---

## Domain Entity

```csharp
using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Core.Guards;

public sealed class Client : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public string PhoneNumber { get; private set; } = default!;

    private Client()
    {
    }

    public Client(
        Guid id,
        string name,
        string email,
        string phoneNumber)
    {
        Guard.NotEmpty(name);
        Guard.NotEmpty(email);
        Guard.NotEmpty(phoneNumber);

        Id = id;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;

        RaiseDomainEvent(
            new ClientCreatedDomainEvent(
                id,
                name,
                email));
    }
}
```

---

## Domain Event

```csharp
using CustomCodeFramework.Core.Domain.Events;

public sealed record ClientCreatedDomainEvent(
    Guid ClientId,
    string Name,
    string Email) : DomainEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class CreateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = new Client(
            Guid.NewGuid(),
            command.Name,
            command.Email,
            command.PhoneNumber);

        await repository.AddAsync(
            client,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return client.Id;
    }
}
```

---

## Endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapPost("/clients", async (
    CreateClientRequest request,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    var clientId = await commandDispatcher.DispatchAsync(
        new CreateClientCommand(
            request.Name,
            request.Email,
            request.PhoneNumber),
        cancellationToken);

    return Results.Created(
        $"/clients/{clientId}",
        new
        {
            ClientId = clientId
        });
});
```

---

# Ejemplo real completo: Consultar cliente

## Query

```csharp
using CustomCodeFramework.Cqrs.Queries;

public sealed record GetClientByIdQuery(Guid ClientId)
    : IQuery<ClientResponse?>;
```

---

## Response

```csharp
public sealed record ClientResponse(
    Guid ClientId,
    string Name,
    string Email,
    string PhoneNumber,
    bool IsActive);
```

---

## Handler usando Dapper

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class GetClientByIdQueryHandler(
    ISqlQueryExecutor queryExecutor)
    : IQueryHandler<GetClientByIdQuery, ClientResponse?>
{
    public Task<ClientResponse?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return queryExecutor.QuerySingleOrDefaultAsync<ClientResponse>(
            """
            select
                client_id as ClientId,
                name as Name,
                email as Email,
                phone_number as PhoneNumber,
                is_active as IsActive
            from clients
            where client_id = @ClientId
            """,
            new
            {
                query.ClientId
            },
            cancellationToken);
    }
}
```

---

## Endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapGet("/clients/{clientId:guid}", async (
    Guid clientId,
    IQueryDispatcher queryDispatcher,
    CancellationToken cancellationToken) =>
{
    var client = await queryDispatcher.DispatchAsync(
        new GetClientByIdQuery(clientId),
        cancellationToken);

    return client is null
        ? Results.NotFound()
        : Results.Ok(client);
});
```

---

# Ejemplo real completo: Actualizar cliente

## Request

```csharp
public sealed record UpdateClientRequest(
    string Name,
    string Email,
    string PhoneNumber);
```

---

## Command

```csharp
using CustomCodeFramework.Cqrs.Commands;

public sealed record UpdateClientCommand(
    Guid ClientId,
    string Name,
    string Email,
    string PhoneNumber) : ICommand;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class UpdateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateClientCommand>
{
    public async Task HandleAsync(
        UpdateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = await repository.GetByIdAsync(
            command.ClientId,
            cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        client.Update(
            command.Name,
            command.Email,
            command.PhoneNumber);

        repository.Update(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
```

---

## Endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapPut("/clients/{clientId:guid}", async (
    Guid clientId,
    UpdateClientRequest request,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    await commandDispatcher.DispatchAsync(
        new UpdateClientCommand(
            clientId,
            request.Name,
            request.Email,
            request.PhoneNumber),
        cancellationToken);

    return Results.NoContent();
});
```

---

# Ejemplo real completo: Buscar clientes paginados

## Query

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Core.Pagination;

public sealed record SearchClientsQuery(
    string? Search,
    int PageNumber,
    int PageSize) : IQuery<PagedResult<ClientResponse>>;
```

---

## Handler

```csharp
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class SearchClientsQueryHandler(
    ISqlQueryExecutor queryExecutor)
    : IQueryHandler<SearchClientsQuery, PagedResult<ClientResponse>>
{
    public async Task<PagedResult<ClientResponse>> HandleAsync(
        SearchClientsQuery query,
        CancellationToken cancellationToken = default)
    {
        var offset = (query.PageNumber - 1) * query.PageSize;

        var items = await queryExecutor.QueryAsync<ClientResponse>(
            """
            select
                client_id as ClientId,
                name as Name,
                email as Email,
                phone_number as PhoneNumber,
                is_active as IsActive
            from clients
            where
                @Search is null
                or name ilike '%' || @Search || '%'
                or email ilike '%' || @Search || '%'
            order by name
            limit @PageSize
            offset @Offset
            """,
            new
            {
                query.Search,
                query.PageSize,
                Offset = offset
            },
            cancellationToken);

        var totalCount = await queryExecutor.QuerySingleAsync<int>(
            """
            select count(*)
            from clients
            where
                @Search is null
                or name ilike '%' || @Search || '%'
                or email ilike '%' || @Search || '%'
            """,
            new
            {
                query.Search
            },
            cancellationToken);

        return new PagedResult<ClientResponse>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}
```

---

## Endpoint

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

app.MapGet("/clients", async (
    string? search,
    int pageNumber,
    int pageSize,
    IQueryDispatcher queryDispatcher,
    CancellationToken cancellationToken) =>
{
    var result = await queryDispatcher.DispatchAsync(
        new SearchClientsQuery(
            search,
            pageNumber,
            pageSize),
        cancellationToken);

    return Results.Ok(result);
});
```

---

# Behaviors

Los behaviors permiten ejecutar lógica común antes o después de los handlers.

Pipeline recomendado:

```text
UnhandledExceptionBehavior
LoggingBehavior
ValidationBehavior
TransactionBehavior
Handler
```

---

# LoggingBehavior

## ¿Qué hace?

Registra:

- Nombre del command/query.
- Inicio de ejecución.
- Fin de ejecución.
- Tiempo de ejecución.
- Errores si ocurren.

---

## Ejemplo conceptual

```csharp
public sealed class LoggingBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling request {RequestName}",
            typeof(TRequest).Name);

        var response = await next();

        logger.LogInformation(
            "Handled request {RequestName}",
            typeof(TRequest).Name);

        return response;
    }
}
```

---

# ValidationBehavior

## ¿Qué hace?

Ejecuta validadores antes del handler.

Evita que el handler tenga esto:

```csharp
if (string.IsNullOrWhiteSpace(command.Name))
{
    throw new Exception("Name is required");
}
```

---

## Validator

```csharp
using FluentValidation;

public sealed class CreateClientCommandValidator
    : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty();
    }
}
```

---

## Flujo

```text
Endpoint
  ↓
CommandDispatcher
  ↓
ValidationBehavior
  ↓
CreateClientCommandValidator
  ↓
CreateClientCommandHandler
```

---

# TransactionBehavior

## ¿Qué hace?

Envuelve commands dentro de una transacción.

Aplica principalmente a commands, no queries.

---

## Flujo

```text
CommandDispatcher
  ↓
TransactionBehavior
  ↓
BeginTransaction
  ↓
Handler
  ↓
Commit
```

Si ocurre error:

```text
CommandDispatcher
  ↓
TransactionBehavior
  ↓
BeginTransaction
  ↓
Handler
  ↓
Exception
  ↓
Rollback
```

---

# UnhandledExceptionBehavior

## ¿Qué hace?

Captura errores no controlados para logging y trazabilidad.

No reemplaza las validaciones de negocio.

---

# Commands vs Queries

## Command

Usar cuando se modifica estado.

```text
CreateClient
UpdateClient
DeleteClient
ApproveOrder
CancelInvoice
GenerateReport
SendNotification
```

Debe llamarse:

```text
Verbo + Entidad + Command
```

Ejemplos:

```csharp
CreateClientCommand
ApproveSalesOrderCommand
CancelInvoiceCommand
UploadClientDocumentCommand
```

---

## Query

Usar cuando se consulta información.

```text
GetClientById
SearchClients
GetInvoiceSummary
GetDashboardMetrics
```

Debe llamarse:

```text
Verbo + Entidad + Query
```

Ejemplos:

```csharp
GetClientByIdQuery
SearchClientsQuery
GetSalesOrderSummaryQuery
GetDashboardMetricsQuery
```

---

# Buenas prácticas

## Commands

Sí:

```csharp
public sealed record CreateClientCommand(
    string Name,
    string Email) : ICommand<Guid>;
```

No:

```csharp
public sealed class CreateClientCommand
{
    public string Name { get; set; }
}
```

---

## Queries

Sí:

```csharp
public sealed record GetClientByIdQuery(Guid ClientId)
    : IQuery<ClientResponse?>;
```

No:

```csharp
public sealed record GetClientQuery
{
    public Guid Id { get; set; }
}
```

---

## Handlers

Sí:

```csharp
public sealed class CreateClientCommandHandler
    : ICommandHandler<CreateClientCommand, Guid>
{
}
```

No:

```csharp
public sealed class ClientService
{
    public Task CreateClient()
    {
    }

    public Task UpdateClient()
    {
    }

    public Task DeleteClient()
    {
    }

    public Task SearchClients()
    {
    }
}
```

---

# Integración con otros paquetes

## CQRS + Validation

```text
Command
  ↓
ValidationBehavior
  ↓
FluentValidation validator
  ↓
Handler
```

---

## CQRS + Persistence

```text
CommandHandler
  ↓
Repository
  ↓
UnitOfWork
```

---

## CQRS + Postgres.Dapper

```text
QueryHandler
  ↓
Dapper query executor
  ↓
Read DTO
```

---

## CQRS + Messaging

```text
CommandHandler
  ↓
Domain event
  ↓
Outbox
  ↓
Integration event
```

---

## CQRS + Redis

```text
QueryHandler
  ↓
Cache lookup
  ↓
Database fallback
  ↓
Cache set
```

---

# Ejemplo: Query con Cache Aside

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class GetClientByIdCachedQueryHandler(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder,
    ISqlQueryExecutor queryExecutor)
    : IQueryHandler<GetClientByIdQuery, ClientResponse?>
{
    public async Task<ClientResponse?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = keyBuilder.Build(
            "clients",
            query.ClientId.ToString());

        var cachedClient = await cacheService.GetAsync<ClientResponse>(
            cacheKey,
            cancellationToken);

        if (cachedClient is not null)
        {
            return cachedClient;
        }

        var client = await queryExecutor.QuerySingleOrDefaultAsync<ClientResponse>(
            """
            select
                client_id as ClientId,
                name as Name,
                email as Email,
                phone_number as PhoneNumber,
                is_active as IsActive
            from clients
            where client_id = @ClientId
            """,
            new
            {
                query.ClientId
            },
            cancellationToken);

        if (client is not null)
        {
            await cacheService.SetAsync(
                cacheKey,
                client,
                TimeSpan.FromMinutes(10),
                cancellationToken);
        }

        return client;
    }
}
```

---

# Ejemplo: Command con evento de integración

## Command

```csharp
public sealed record ApproveClientCommand(Guid ClientId) : ICommand;
```

---

## Integration Event

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("client-approved")]
public sealed record ClientApprovedIntegrationEvent(
    Guid ClientId,
    DateTime ApprovedAtUtc) : IntegrationEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class ApproveClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<ApproveClientCommand>
{
    public async Task HandleAsync(
        ApproveClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = await repository.GetByIdAsync(
            command.ClientId,
            cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        client.Approve();

        repository.Update(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        await eventPublisher.PublishAsync(
            new ClientApprovedIntegrationEvent(
                client.Id,
                DateTime.UtcNow),
            cancellationToken);
    }
}
```

---

# Ejemplo de organización por feature

```text
Features
└── Clients
    ├── CreateClient
    │   ├── CreateClientCommand.cs
    │   ├── CreateClientCommandHandler.cs
    │   ├── CreateClientCommandValidator.cs
    │   └── CreateClientEndpoint.cs
    │
    ├── UpdateClient
    │   ├── UpdateClientCommand.cs
    │   ├── UpdateClientCommandHandler.cs
    │   ├── UpdateClientCommandValidator.cs
    │   └── UpdateClientEndpoint.cs
    │
    ├── DeleteClient
    │   ├── DeleteClientCommand.cs
    │   ├── DeleteClientCommandHandler.cs
    │   └── DeleteClientEndpoint.cs
    │
    └── GetClientById
        ├── GetClientByIdQuery.cs
        ├── GetClientByIdQueryHandler.cs
        └── GetClientByIdEndpoint.cs
```

---

# Organización alternativa por capas

```text
Application
├── Clients
│   ├── Commands
│   │   ├── CreateClientCommand.cs
│   │   ├── UpdateClientCommand.cs
│   │   └── DeleteClientCommand.cs
│   │
│   ├── Queries
│   │   ├── GetClientByIdQuery.cs
│   │   └── SearchClientsQuery.cs
│   │
│   ├── Handlers
│   │   ├── CreateClientCommandHandler.cs
│   │   ├── UpdateClientCommandHandler.cs
│   │   ├── DeleteClientCommandHandler.cs
│   │   ├── GetClientByIdQueryHandler.cs
│   │   └── SearchClientsQueryHandler.cs
│   │
│   └── Responses
│       └── ClientResponse.cs
```

---

# Recomendación

Para microservicios o sistemas grandes, usar organización por feature:

```text
Features/Clients/CreateClient
Features/Clients/GetClientById
Features/Clients/SearchClients
```

Es más fácil de mantener y escalar.

---

# Errores comunes

## Usar command para consultar

Incorrecto:

```csharp
public sealed record GetClientCommand(Guid ClientId) : ICommand<ClientResponse>;
```

Correcto:

```csharp
public sealed record GetClientByIdQuery(Guid ClientId) : IQuery<ClientResponse>;
```

---

## Modificar estado desde query

Incorrecto:

```csharp
public sealed class GetClientByIdQueryHandler
{
    public async Task<ClientResponse> HandleAsync(...)
    {
        client.LastViewedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
```

Correcto:

```csharp
public sealed class GetClientByIdQueryHandler
{
    public async Task<ClientResponse> HandleAsync(...)
    {
        return await readRepository.GetAsync(...);
    }
}
```

---

## Poner lógica de negocio en endpoint

Incorrecto:

```csharp
app.MapPost("/clients", async (CreateClientRequest request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest();
    }

    var client = new Client(...);

    db.Clients.Add(client);

    await db.SaveChangesAsync();

    return Results.Ok();
});
```

Correcto:

```csharp
app.MapPost("/clients", async (
    CreateClientRequest request,
    ICommandDispatcher dispatcher,
    CancellationToken cancellationToken) =>
{
    var clientId = await dispatcher.DispatchAsync(
        new CreateClientCommand(
            request.Name,
            request.Email,
            request.PhoneNumber),
        cancellationToken);

    return Results.Created($"/clients/{clientId}", new { ClientId = clientId });
});
```

---

# Flujo recomendado de una petición

## Crear cliente

```text
HTTP POST /clients
  ↓
Endpoint
  ↓
CreateClientCommand
  ↓
CommandDispatcher
  ↓
UnhandledExceptionBehavior
  ↓
LoggingBehavior
  ↓
ValidationBehavior
  ↓
TransactionBehavior
  ↓
CreateClientCommandHandler
  ↓
Repository
  ↓
UnitOfWork
  ↓
Domain Events
  ↓
Outbox
  ↓
Response 201 Created
```

---

# Resumen

Usar `CustomCodeFramework.Cqrs` cuando se quiera:

- Separar escritura y lectura.
- Tener handlers pequeños.
- Centralizar validación.
- Centralizar logging.
- Centralizar transacciones.
- Evitar servicios enormes.
- Preparar el sistema para eventos, outbox, cache y read models.

CQRS debe ser el punto de entrada de los casos de uso de la aplicación.
