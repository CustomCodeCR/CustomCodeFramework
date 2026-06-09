# CustomCodeFramework.Mongo

## Introducción

`CustomCodeFramework.Mongo` es el paquete encargado de integrar MongoDB dentro de CustomCodeFramework.

Este paquete está pensado principalmente para:

- Read models.
- Proyecciones.
- Consultas rápidas.
- Vistas materializadas.
- Documentos denormalizados.
- Reportes livianos.
- Datos de lectura para CQRS.
- Persistencia documental cuando el modelo encaja mejor como documento.

MongoDB no debe reemplazar automáticamente a PostgreSQL. Se recomienda usarlo cuando el caso de uso necesita documentos flexibles, proyecciones o lecturas rápidas.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Mongo --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "customcode",
    "EnableTransactions": false
  }
}
```

Con usuario y contraseña:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://admin:admin@localhost:27017",
    "DatabaseName": "customcode",
    "EnableTransactions": false
  }
}
```

Con replica set:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017/?replicaSet=rs0",
    "DatabaseName": "customcode",
    "EnableTransactions": true
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Mongo.DependencyInjection;

builder.Services.AddCustomCodeMongo(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Mongo
├── Options
│   └── MongoOptions.cs
│
├── Abstractions
│   ├── IMongoContext.cs
│   ├── IMongoCollectionProvider.cs
│   └── IReadModel.cs
│
├── Collections
│   ├── MongoCollectionNameAttribute.cs
│   └── MongoCollectionRegistry.cs
│
├── Repositories
│   ├── MongoRepository.cs
│   └── MongoReadRepository.cs
│
├── Projections
│   ├── IProjection.cs
│   ├── IProjector.cs
│   └── MongoProjectionStore.cs
│
├── Indexes
│   ├── IMongoIndexBuilder.cs
│   └── MongoIndexRunner.cs
│
├── Transactions
│   └── MongoTransactionManager.cs
│
├── HealthChecks
│   └── MongoHealthCheck.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# ¿Cuándo usar Mongo?

Usar MongoDB para:

```text
read models
proyecciones CQRS
vistas denormalizadas
consultas rápidas
documentos flexibles
historiales
logs funcionales
snapshots
dashboards simples
```

Ejemplos:

```text
ClientReadModel
SalesOrderReadModel
InvoiceReadModel
DiscountHistoryReadModel
NotificationDeliveryReadModel
AuditLogReadModel
ReportJobReadModel
```

---

# ¿Cuándo no usar Mongo?

No usar MongoDB como primera opción para:

```text
transacciones relacionales complejas
agregados con invariantes fuertes
contabilidad
facturación fiscal
inventario crítico
relaciones altamente normalizadas
```

Para eso usar:

```text
PostgreSQL + EntityFramework
```

---

# MongoOptions

Configuración base:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "customcode",
    "EnableTransactions": false
  }
}
```

---

# IReadModel

## ¿Qué es?

`IReadModel` marca una clase como modelo de lectura.

Un read model no es una entidad de dominio.

No debe contener reglas de negocio.

Debe estar optimizado para mostrar datos.

---

## Ejemplo

```csharp
using CustomCodeFramework.Mongo.Abstractions;

public sealed class ClientReadModel : IReadModel
{
    public string Id { get; init; } = default!;

    public Guid ClientId { get; init; }

    public string Name { get; init; } = default!;

    public string Email { get; init; } = default!;

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
```

---

# MongoCollectionNameAttribute

## ¿Para qué sirve?

Define el nombre de la colección donde se guardará el documento.

---

## Ejemplo

```csharp
using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

[MongoCollectionName("clients")]
public sealed class ClientReadModel : IReadModel
{
    public string Id { get; init; } = default!;

    public Guid ClientId { get; init; }

    public string Name { get; init; } = default!;

    public string Email { get; init; } = default!;
}
```

Resultado:

```text
Database: customcode
Collection: clients
```

---

# Convención recomendada para Id

Mongo usa normalmente `_id`.

En el framework se recomienda usar:

```csharp
public string Id { get; init; } = default!;
```

Ejemplos:

```text
client:{ClientId}
sales_order:{SalesOrderId}
invoice:{InvoiceId}
discount_history:{HistoryId}
notification_delivery:{DeliveryId}
```

---

# Ejemplo

```csharp
var readModel = new ClientReadModel
{
    Id = $"client:{clientId}",
    ClientId = clientId,
    Name = "ACME",
    Email = "client@example.com",
    IsActive = true,
    CreatedAtUtc = DateTime.UtcNow
};
```

---

# IMongoContext

## ¿Qué es?

Abstracción sobre la base Mongo.

Permite acceder a la base sin depender directamente del cliente en la aplicación.

---

## Uso conceptual

```csharp
using CustomCodeFramework.Mongo.Abstractions;

public sealed class MongoInfoService(IMongoContext context)
{
    public string GetDatabaseName()
    {
        return context.Database.DatabaseNamespace.DatabaseName;
    }
}
```

---

# IMongoCollectionProvider

## ¿Qué es?

Proveedor de colecciones Mongo.

Sirve para obtener la colección correcta según el tipo de read model.

---

## Uso

```csharp
using CustomCodeFramework.Mongo.Abstractions;

public sealed class ClientCollectionService(
    IMongoCollectionProvider collectionProvider)
{
    public IMongoCollection<ClientReadModel> GetCollection()
    {
        return collectionProvider.GetCollection<ClientReadModel>();
    }
}
```

---

# MongoRepository

## ¿Para qué sirve?

Repositorio de escritura para documentos Mongo.

Usarlo cuando se necesita insertar, actualizar o eliminar read models/documentos.

---

## Insertar documento

```csharp
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientReadModelWriter(
    MongoRepository<ClientReadModel> repository)
{
    public Task InsertAsync(
        Guid clientId,
        string name,
        string email,
        CancellationToken cancellationToken)
    {
        var readModel = new ClientReadModel
        {
            Id = $"client:{clientId}",
            ClientId = clientId,
            Name = name,
            Email = email,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return repository.InsertAsync(
            readModel,
            cancellationToken);
    }
}
```

---

## Actualizar documento

```csharp
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientReadModelUpdater(
    MongoRepository<ClientReadModel> repository)
{
    public Task UpdateAsync(
        ClientReadModel readModel,
        CancellationToken cancellationToken)
    {
        return repository.UpdateAsync(
            readModel.Id,
            readModel,
            cancellationToken);
    }
}
```

---

## Eliminar documento

```csharp
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientReadModelDeleter(
    MongoRepository<ClientReadModel> repository)
{
    public Task DeleteAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(
            $"client:{clientId}",
            cancellationToken);
    }
}
```

---

# MongoReadRepository

## ¿Para qué sirve?

Repositorio para leer documentos Mongo.

Usarlo en QueryHandlers o servicios de lectura.

---

## Buscar por Id

```csharp
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientReadService(
    MongoReadRepository<ClientReadModel> repository)
{
    public Task<ClientReadModel?> GetByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        return repository.GetByIdAsync(
            $"client:{clientId}",
            cancellationToken);
    }
}
```

---

## Listar documentos

```csharp
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientReadService(
    MongoReadRepository<ClientReadModel> repository)
{
    public Task<IReadOnlyCollection<ClientReadModel>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return repository.ListAsync(
            cancellationToken);
    }
}
```

---

# Ejemplo CQRS: Query desde Mongo

## Query

```csharp
using CustomCodeFramework.Cqrs.Queries;

public sealed record GetClientByIdQuery(Guid ClientId)
    : IQuery<ClientReadModel?>;
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Mongo.Repositories;

public sealed class GetClientByIdQueryHandler(
    MongoReadRepository<ClientReadModel> repository)
    : IQueryHandler<GetClientByIdQuery, ClientReadModel?>
{
    public Task<ClientReadModel?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByIdAsync(
            $"client:{query.ClientId}",
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

# Ejemplo CQRS: Búsqueda de clientes

## Query

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Core.Pagination;

public sealed record SearchClientsQuery(
    string? Search,
    int PageNumber,
    int PageSize) : IQuery<PagedResult<ClientReadModel>>;
```

---

## Handler usando Mongo Driver

```csharp
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Mongo.Abstractions;
using MongoDB.Driver;

public sealed class SearchClientsQueryHandler(
    IMongoCollectionProvider collectionProvider)
    : IQueryHandler<SearchClientsQuery, PagedResult<ClientReadModel>>
{
    public async Task<PagedResult<ClientReadModel>> HandleAsync(
        SearchClientsQuery query,
        CancellationToken cancellationToken = default)
    {
        var collection = collectionProvider.GetCollection<ClientReadModel>();

        var filterBuilder = Builders<ClientReadModel>.Filter;

        var filter = string.IsNullOrWhiteSpace(query.Search)
            ? filterBuilder.Empty
            : filterBuilder.Or(
                filterBuilder.Regex(
                    x => x.Name,
                    new MongoDB.Bson.BsonRegularExpression(query.Search, "i")),
                filterBuilder.Regex(
                    x => x.Email,
                    new MongoDB.Bson.BsonRegularExpression(query.Search, "i")));

        var totalCount = await collection.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);

        var items = await collection
            .Find(filter)
            .SortBy(x => x.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientReadModel>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}
```

---

# Projections

## ¿Qué es una proyección?

Una proyección transforma eventos del dominio o eventos de integración en read models.

Ejemplo:

```text
ClientCreatedDomainEvent
    ↓
ClientReadModel
```

---

# IProjection

Representa una proyección.

---

# IProjector

Procesa eventos específicos.

---

## Evento de dominio

```csharp
using CustomCodeFramework.Core.Domain.Events;

public sealed record ClientCreatedDomainEvent(
    Guid ClientId,
    string Name,
    string Email) : DomainEvent;
```

---

## Read Model

```csharp
using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

[MongoCollectionName("clients")]
public sealed class ClientReadModel : IReadModel
{
    public string Id { get; init; } = default!;

    public Guid ClientId { get; init; }

    public string Name { get; init; } = default!;

    public string Email { get; init; } = default!;

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
```

---

## Projector

```csharp
using CustomCodeFramework.Mongo.Projections;
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientProjection(
    MongoRepository<ClientReadModel> repository)
    : IProjector<ClientCreatedDomainEvent>
{
    public Task ProjectAsync(
        ClientCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var readModel = new ClientReadModel
        {
            Id = $"client:{domainEvent.ClientId}",
            ClientId = domainEvent.ClientId,
            Name = domainEvent.Name,
            Email = domainEvent.Email,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return repository.InsertAsync(
            readModel,
            cancellationToken);
    }
}
```

---

# Proyección de actualización

## Evento

```csharp
public sealed record ClientUpdatedDomainEvent(
    Guid ClientId,
    string Name,
    string Email) : DomainEvent;
```

---

## Projector

```csharp
using CustomCodeFramework.Mongo.Projections;
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientUpdatedProjection(
    MongoRepository<ClientReadModel> repository,
    MongoReadRepository<ClientReadModel> readRepository)
    : IProjector<ClientUpdatedDomainEvent>
{
    public async Task ProjectAsync(
        ClientUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var existing = await readRepository.GetByIdAsync(
            $"client:{domainEvent.ClientId}",
            cancellationToken);

        if (existing is null)
        {
            return;
        }

        var updated = existing with
        {
            Name = domainEvent.Name,
            Email = domainEvent.Email
        };

        await repository.UpdateAsync(
            updated.Id,
            updated,
            cancellationToken);
    }
}
```

Si el read model no es record, hacerlo así:

```csharp
var updated = new ClientReadModel
{
    Id = existing.Id,
    ClientId = existing.ClientId,
    Name = domainEvent.Name,
    Email = domainEvent.Email,
    IsActive = existing.IsActive,
    CreatedAtUtc = existing.CreatedAtUtc
};
```

---

# Proyección de eliminación

## Evento

```csharp
public sealed record ClientDeletedDomainEvent(
    Guid ClientId) : DomainEvent;
```

---

## Projector

```csharp
using CustomCodeFramework.Mongo.Projections;
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientDeletedProjection(
    MongoRepository<ClientReadModel> repository)
    : IProjector<ClientDeletedDomainEvent>
{
    public Task ProjectAsync(
        ClientDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        return repository.DeleteAsync(
            $"client:{domainEvent.ClientId}",
            cancellationToken);
    }
}
```

---

# MongoProjectionStore

## ¿Para qué sirve?

Permite guardar o actualizar proyecciones de forma centralizada.

Uso conceptual:

```csharp
using CustomCodeFramework.Mongo.Projections;

public sealed class ProjectionService(
    MongoProjectionStore projectionStore)
{
    public Task SaveAsync(
        ClientReadModel readModel,
        CancellationToken cancellationToken)
    {
        return projectionStore.UpsertAsync(
            readModel.Id,
            readModel,
            cancellationToken);
    }
}
```

---

# Ejemplo completo: SalesOrderReadModel

## Read Model

```csharp
using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

[MongoCollectionName("sales_orders")]
public sealed class SalesOrderReadModel : IReadModel
{
    public string Id { get; init; } = default!;

    public Guid SalesOrderId { get; init; }

    public string OrderNumber { get; init; } = default!;

    public Guid ClientId { get; init; }

    public string ClientName { get; init; } = default!;

    public DateTime CreatedAtUtc { get; init; }

    public string Status { get; init; } = default!;

    public decimal Subtotal { get; init; }

    public decimal Taxes { get; init; }

    public decimal Total { get; init; }

    public IReadOnlyCollection<SalesOrderLineReadModel> Lines { get; init; } = [];
}
```

---

## Line Read Model

```csharp
public sealed class SalesOrderLineReadModel
{
    public Guid ProductId { get; init; }

    public string ProductCode { get; init; } = default!;

    public string ProductName { get; init; } = default!;

    public decimal Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal Total { get; init; }
}
```

---

## Query por ID

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Mongo.Repositories;

public sealed record GetSalesOrderByIdQuery(Guid SalesOrderId)
    : IQuery<SalesOrderReadModel?>;

public sealed class GetSalesOrderByIdQueryHandler(
    MongoReadRepository<SalesOrderReadModel> repository)
    : IQueryHandler<GetSalesOrderByIdQuery, SalesOrderReadModel?>
{
    public Task<SalesOrderReadModel?> HandleAsync(
        GetSalesOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByIdAsync(
            $"sales_order:{query.SalesOrderId}",
            cancellationToken);
    }
}
```

---

# Ejemplo: Historial de descuentos

## Read Model

```csharp
using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

[MongoCollectionName("discount_history")]
public sealed class DiscountHistoryReadModel : IReadModel
{
    public string Id { get; init; } = default!;

    public Guid HistoryId { get; init; }

    public Guid SalesOrderId { get; init; }

    public string SalesOrderCode { get; init; } = default!;

    public string Action { get; init; } = default!;

    public decimal? PreviousDiscount { get; init; }

    public decimal? NewDiscount { get; init; }

    public string RequestedBy { get; init; } = default!;

    public string? ApprovedBy { get; init; }

    public string? Reason { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
```

---

## Insertar historial

```csharp
using CustomCodeFramework.Mongo.Repositories;

public sealed class DiscountHistoryWriter(
    MongoRepository<DiscountHistoryReadModel> repository)
{
    public Task AddAsync(
        Guid salesOrderId,
        string salesOrderCode,
        string action,
        decimal? previousDiscount,
        decimal? newDiscount,
        string requestedBy,
        string? approvedBy,
        string? reason,
        CancellationToken cancellationToken)
    {
        var historyId = Guid.NewGuid();

        return repository.InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{historyId}",
                HistoryId = historyId,
                SalesOrderId = salesOrderId,
                SalesOrderCode = salesOrderCode,
                Action = action,
                PreviousDiscount = previousDiscount,
                NewDiscount = newDiscount,
                RequestedBy = requestedBy,
                ApprovedBy = approvedBy,
                Reason = reason,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }
}
```

---

# Indexes

## ¿Para qué sirven?

Mongo necesita índices para que las consultas sean rápidas.

Ejemplos:

```text
email
client_id
sales_order_id
created_at_utc
status
```

---

# IMongoIndexBuilder

Permite declarar índices por read model.

---

## Ejemplo índices de clientes

```csharp
using CustomCodeFramework.Mongo.Indexes;
using MongoDB.Driver;

public sealed class ClientReadModelIndexBuilder
    : IMongoIndexBuilder<ClientReadModel>
{
    public IEnumerable<CreateIndexModel<ClientReadModel>> BuildIndexes()
    {
        yield return new CreateIndexModel<ClientReadModel>(
            Builders<ClientReadModel>.IndexKeys
                .Ascending(x => x.ClientId),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_clients_client_id"
            });

        yield return new CreateIndexModel<ClientReadModel>(
            Builders<ClientReadModel>.IndexKeys
                .Ascending(x => x.Email),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_clients_email"
            });

        yield return new CreateIndexModel<ClientReadModel>(
            Builders<ClientReadModel>.IndexKeys
                .Ascending(x => x.Name),
            new CreateIndexOptions
            {
                Name = "ix_clients_name"
            });
    }
}
```

---

## Ejecutar índices al iniciar

```csharp
using CustomCodeFramework.Mongo.Indexes;

var app = builder.Build();

await app.Services
    .GetRequiredService<MongoIndexRunner>()
    .RunAsync(CancellationToken.None);

app.Run();
```

---

# Índices para SalesOrderReadModel

```csharp
using CustomCodeFramework.Mongo.Indexes;
using MongoDB.Driver;

public sealed class SalesOrderReadModelIndexBuilder
    : IMongoIndexBuilder<SalesOrderReadModel>
{
    public IEnumerable<CreateIndexModel<SalesOrderReadModel>> BuildIndexes()
    {
        yield return new CreateIndexModel<SalesOrderReadModel>(
            Builders<SalesOrderReadModel>.IndexKeys
                .Ascending(x => x.SalesOrderId),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_sales_orders_sales_order_id"
            });

        yield return new CreateIndexModel<SalesOrderReadModel>(
            Builders<SalesOrderReadModel>.IndexKeys
                .Ascending(x => x.ClientId),
            new CreateIndexOptions
            {
                Name = "ix_sales_orders_client_id"
            });

        yield return new CreateIndexModel<SalesOrderReadModel>(
            Builders<SalesOrderReadModel>.IndexKeys
                .Ascending(x => x.Status),
            new CreateIndexOptions
            {
                Name = "ix_sales_orders_status"
            });

        yield return new CreateIndexModel<SalesOrderReadModel>(
            Builders<SalesOrderReadModel>.IndexKeys
                .Descending(x => x.CreatedAtUtc),
            new CreateIndexOptions
            {
                Name = "ix_sales_orders_created_at_utc"
            });
    }
}
```

---

# Health Check

## Registro

```csharp
using CustomCodeFramework.Mongo.DependencyInjection;

builder.Services.AddCustomCodeMongo(builder.Configuration);
builder.Services.AddCustomCodeMongoHealthChecks(builder.Configuration);
```

---

## Endpoint

```csharp
app.MapHealthChecks("/health/mongo");
```

---

# Transacciones Mongo

Mongo soporta transacciones si se usa replica set.

Configuración:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017/?replicaSet=rs0",
    "DatabaseName": "customcode",
    "EnableTransactions": true
  }
}
```

---

## Uso

```csharp
using CustomCodeFramework.Mongo.Transactions;

public sealed class MongoTransactionalService(
    MongoTransactionManager transactionManager,
    MongoRepository<ClientReadModel> clientRepository,
    MongoRepository<DiscountHistoryReadModel> historyRepository)
{
    public Task ExecuteAsync(
        ClientReadModel client,
        DiscountHistoryReadModel history,
        CancellationToken cancellationToken)
    {
        return transactionManager.ExecuteAsync(
            async ct =>
            {
                await clientRepository.InsertAsync(client, ct);
                await historyRepository.InsertAsync(history, ct);
            },
            cancellationToken);
    }
}
```

---

# Integración con CQRS

## Query usando Mongo

```text
HTTP GET /clients/{id}
    ↓
GetClientByIdQuery
    ↓
GetClientByIdQueryHandler
    ↓
MongoReadRepository<ClientReadModel>
    ↓
ClientReadModel
```

---

## Projection usando Mongo

```text
ClientCreatedDomainEvent
    ↓
ClientProjection
    ↓
MongoRepository<ClientReadModel>
    ↓
clients collection
```

---

# Integración con PostgreSQL

Uso recomendado:

```text
PostgreSQL + EF Core -> escritura principal
MongoDB -> read models
```

Flujo:

```text
Command
    ↓
Aggregate
    ↓
PostgreSQL
    ↓
Domain Event
    ↓
Outbox
    ↓
Projector
    ↓
Mongo ReadModel
```

---

# Ejemplo completo: Escritura en Postgres y lectura en Mongo

## Command

```csharp
public sealed record CreateClientCommand(
    string Name,
    string Email) : ICommand<Guid>;
```

---

## Handler de escritura

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
            command.Email);

        await repository.AddAsync(client, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}
```

---

## Evento

```csharp
public sealed record ClientCreatedDomainEvent(
    Guid ClientId,
    string Name,
    string Email) : DomainEvent;
```

---

## Projector Mongo

```csharp
using CustomCodeFramework.Mongo.Projections;
using CustomCodeFramework.Mongo.Repositories;

public sealed class ClientCreatedProjector(
    MongoRepository<ClientReadModel> repository)
    : IProjector<ClientCreatedDomainEvent>
{
    public Task ProjectAsync(
        ClientCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        return repository.InsertAsync(
            new ClientReadModel
            {
                Id = $"client:{domainEvent.ClientId}",
                ClientId = domainEvent.ClientId,
                Name = domainEvent.Name,
                Email = domainEvent.Email,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }
}
```

---

## Query desde Mongo

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Mongo.Repositories;

public sealed record GetClientByIdQuery(Guid ClientId)
    : IQuery<ClientReadModel?>;

public sealed class GetClientByIdQueryHandler(
    MongoReadRepository<ClientReadModel> repository)
    : IQueryHandler<GetClientByIdQuery, ClientReadModel?>
{
    public Task<ClientReadModel?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByIdAsync(
            $"client:{query.ClientId}",
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
│   │   └── CreateClientCommandHandler.cs
│   │
│   └── GetClientById
│       ├── GetClientByIdQuery.cs
│       └── GetClientByIdQueryHandler.cs
│
Infrastructure
├── Mongo
│   ├── ReadModels
│   │   ├── ClientReadModel.cs
│   │   ├── SalesOrderReadModel.cs
│   │   └── DiscountHistoryReadModel.cs
│   │
│   ├── Projections
│   │   ├── ClientCreatedProjector.cs
│   │   ├── ClientUpdatedProjector.cs
│   │   └── ClientDeletedProjector.cs
│   │
│   └── Indexes
│       ├── ClientReadModelIndexBuilder.cs
│       └── SalesOrderReadModelIndexBuilder.cs
```

---

# Program.cs completo

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();

builder.Services.AddCustomCodeMongo(builder.Configuration);

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "Clients.Api",
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

# Docker Compose de ejemplo

```yaml
services:
  mongo:
    image: mongo:7
    container_name: customcode-mongo
    restart: unless-stopped
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_DATABASE: customcode
    volumes:
      - mongo_data:/data/db

volumes:
  mongo_data:
```

Con usuario:

```yaml
services:
  mongo:
    image: mongo:7
    container_name: customcode-mongo
    restart: unless-stopped
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: admin
      MONGO_INITDB_DATABASE: customcode
    volumes:
      - mongo_data:/data/db

volumes:
  mongo_data:
```

Connection string:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://admin:admin@localhost:27017",
    "DatabaseName": "customcode"
  }
}
```

---

# Buenas prácticas

## Sí

Usar Mongo para read models:

```csharp
ClientReadModel
SalesOrderReadModel
InvoiceReadModel
```

Usar Id determinístico:

```csharp
Id = $"client:{clientId}"
```

Crear índices:

```csharp
IMongoIndexBuilder<ClientReadModel>
```

Separar modelos de dominio y modelos de lectura:

```text
Client -> dominio
ClientReadModel -> lectura
```

---

## No

No meter lógica de negocio en read models:

```csharp
public void Approve()
{
}
```

No usar Mongo como reemplazo automático de PostgreSQL.

No guardar agregados críticos sin evaluar consistencia.

No hacer consultas sin índices en colecciones grandes.

No guardar secretos en documentos de lectura.

---

# Errores comunes

## Usar Guid como Id sin prefijo

No recomendado:

```csharp
Id = clientId.ToString();
```

Recomendado:

```csharp
Id = $"client:{clientId}";
```

---

## No crear índices

Problema:

```text
colección crece
consulta se vuelve lenta
CPU alta
timeouts
```

Solución:

```csharp
IMongoIndexBuilder<TReadModel>
```

---

## Usar read model como entidad de dominio

Incorrecto:

```csharp
public sealed class ClientReadModel
{
    public void ChangeEmail(string email)
    {
        Email = email;
    }
}
```

Correcto:

```csharp
public sealed class ClientReadModel
{
    public string Email { get; init; } = default!;
}
```

---

## Hacer joins manuales entre colecciones

Mongo no debe usarse como si fuera SQL.

Si se necesita join constante, denormalizar:

```csharp
public sealed class SalesOrderReadModel
{
    public Guid ClientId { get; init; }

    public string ClientName { get; init; } = default!;
}
```

---

# Read Model vs Entity

## Entity

```text
vive en dominio
tiene reglas de negocio
protege invariantes
emite domain events
se persiste normalmente en PostgreSQL
```

## ReadModel

```text
vive en infraestructura/application read side
no tiene reglas de negocio
está optimizado para lectura
puede estar denormalizado
se reconstruye desde eventos
```

---

# Flujo recomendado

```text
Command
    ↓
Aggregate
    ↓
PostgreSQL
    ↓
Domain Event
    ↓
Outbox
    ↓
Projector
    ↓
MongoDB
    ↓
Query
    ↓
Read Model
```

---

# Resumen

`CustomCodeFramework.Mongo` debe usarse para:

```text
read models
projections
consultas rápidas
vistas denormalizadas
historiales
dashboards simples
documentos flexibles
```

No debe usarse para reemplazar todas las bases relacionales.

Combinación recomendada:

```text
PostgreSQL + EF Core -> escritura y consistencia
MongoDB -> lectura y proyecciones
Redis -> cache temporal
Dapper -> queries SQL optimizadas
```
