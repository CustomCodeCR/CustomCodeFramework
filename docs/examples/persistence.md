# CustomCodeFramework.Persistence

## Introducción

`CustomCodeFramework.Persistence` es el paquete base para definir contratos de persistencia sin amarrar la aplicación a una tecnología específica.

Este paquete no debe saber si se usa:

- Entity Framework Core.
- Dapper.
- PostgreSQL.
- SQL Server.
- MongoDB.
- Redis.
- Archivos.
- Otro proveedor.

Su responsabilidad es definir las abstracciones comunes para trabajar con:

- Repositorios.
- Unit of Work.
- Read repositories.
- Transacciones.
- Connection factories.
- Specifications.
- Resultados transaccionales.

---

# ¿Qué problema resuelve?

Sin una capa de persistencia base, los casos de uso terminan dependiendo directamente de infraestructura:

```csharp
public sealed class CreateClientCommandHandler(AppDbContext dbContext)
{
    public async Task<Guid> HandleAsync(CreateClientCommand command)
    {
        var client = new Client(...);

        dbContext.Clients.Add(client);

        await dbContext.SaveChangesAsync();

        return client.Id;
    }
}
```

Problemas:

```text
El handler depende directamente de EF Core.
No se puede cambiar fácilmente la implementación.
No se puede probar fácilmente.
Se mezcla aplicación con infraestructura.
No hay contratos comunes.
```

---

# Solución

```text
Application
    ↓
IRepository
IReadRepository
IUnitOfWork
ITransactionManager
    ↓
Infrastructure
    ↓
EF Core / Dapper / Mongo / Postgres
```

El handler depende de abstracciones:

```csharp
public sealed class CreateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
{
}
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Persistence --version 0.1.0-alpha.1
```

---

# Registro

```csharp
using CustomCodeFramework.Persistence.DependencyInjection;

builder.Services.AddCustomCodePersistence();
```

---

# Estructura

```text
CustomCodeFramework.Persistence
├── Abstractions
│   ├── IUnitOfWork.cs
│   ├── IRepository.cs
│   ├── IReadRepository.cs
│   ├── ITransactionManager.cs
│   └── IConnectionFactory.cs
│
├── Specifications
│   ├── ISpecification.cs
│   ├── Specification.cs
│   └── SpecificationEvaluator.cs
│
├── Transactions
│   ├── TransactionScopeOptions.cs
│   └── TransactionResult.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# Responsabilidades del paquete

Este paquete define contratos para:

```text
Agregar entidades
Actualizar entidades
Eliminar entidades
Consultar entidades
Guardar cambios
Ejecutar transacciones
Evaluar specifications
Crear conexiones
Separar lectura y escritura
```

No implementa directamente la persistencia final.

Las implementaciones concretas deben vivir en paquetes como:

```text
CustomCodeFramework.Postgres.EntityFramework
CustomCodeFramework.Postgres.Dapper
CustomCodeFramework.Mongo
```

---

# IUnitOfWork

## ¿Qué es?

`IUnitOfWork` representa una unidad de trabajo.

Agrupa varios cambios y los confirma juntos.

---

## Problema sin UnitOfWork

```csharp
await clientRepository.AddAsync(client);
await orderRepository.AddAsync(order);
await invoiceRepository.AddAsync(invoice);
```

Si algo falla en medio, el sistema puede quedar inconsistente.

---

## Solución

```csharp
await clientRepository.AddAsync(client, cancellationToken);
await orderRepository.AddAsync(order, cancellationToken);
await invoiceRepository.AddAsync(invoice, cancellationToken);

await unitOfWork.SaveChangesAsync(cancellationToken);
```

---

## Uso básico

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class CreateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var client = new Client(
            Guid.NewGuid(),
            command.Name,
            command.Email);

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

# IRepository

## ¿Qué es?

`IRepository` representa un repositorio de escritura.

Se usa para trabajar con agregados o entidades que modifican estado.

---

## Uso recomendado

```text
Commands -> IRepository
Queries  -> IReadRepository / Dapper / Mongo / Read Models
```

---

## Métodos típicos

```text
GetByIdAsync
AddAsync
Update
Delete
Remove
```

---

## Ejemplo de entidad

```csharp
using CustomCodeFramework.Core.Domain.Entities;

public sealed class Client : AggregateRoot<Guid>
{
    public string Name { get; private set; }

    public string Email { get; private set; }

    private Client()
    {
    }

    public Client(
        Guid id,
        string name,
        string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public void Update(
        string name,
        string email)
    {
        Name = name;
        Email = email;
    }
}
```

---

## Crear entidad

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class CreateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var client = new Client(
            Guid.NewGuid(),
            command.Name,
            command.Email);

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

## Actualizar entidad

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class UpdateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        UpdateClientCommand command,
        CancellationToken cancellationToken)
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
            command.Email);

        repository.Update(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
```

---

## Eliminar entidad

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class DeleteClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        DeleteClientCommand command,
        CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(
            command.ClientId,
            cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        repository.Delete(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
```

---

# IReadRepository

## ¿Qué es?

`IReadRepository` representa un repositorio de lectura.

Debe usarse cuando solo se necesita consultar datos.

---

## Diferencia con IRepository

```text
IRepository      -> escritura
IReadRepository  -> lectura
```

---

## Uso recomendado

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class GetClientByIdQueryHandler(
    IReadRepository<Client, Guid> repository)
{
    public async Task<ClientResponse?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(
            query.ClientId,
            cancellationToken);

        if (client is null)
        {
            return null;
        }

        return new ClientResponse(
            client.Id,
            client.Name,
            client.Email);
    }
}
```

---

# ¿Cuándo usar IReadRepository?

Usar para:

```text
consultas simples por ID
consultas por specification
validaciones de existencia
lecturas del dominio
```

No usar para:

```text
reportes pesados
dashboards
consultas con joins complejos
consultas optimizadas
```

Para esas consultas usar mejor:

```text
Dapper
Mongo read models
Vistas SQL
Materialized views
```

---

# IConnectionFactory

## ¿Qué es?

Contrato para crear conexiones.

Lo usan proveedores como:

```text
Postgres
Dapper
SQL Server
```

---

## Uso conceptual

```csharp
using CustomCodeFramework.Persistence.Abstractions;
using System.Data;

public sealed class ClientQueries(IConnectionFactory connectionFactory)
{
    public async Task<ClientResponse?> GetAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = connectionFactory.CreateConnection();

        // Ejecutar consulta.
        return null;
    }
}
```

---

# ITransactionManager

## ¿Qué es?

Contrato para manejar transacciones explícitas.

Debe usarse cuando un caso necesita control manual de transacción.

---

## Ejemplo

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class ApproveOrderService(
    ITransactionManager transactionManager)
{
    public async Task ApproveAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await transactionManager.ExecuteAsync(
            async ct =>
            {
                // 1. Aprobar orden.
                // 2. Generar factura.
                // 3. Registrar movimiento.
                // 4. Guardar cambios.
            },
            cancellationToken);
    }
}
```

---

# Specifications

## ¿Qué es una Specification?

Una specification encapsula criterios de consulta.

Evita repetir filtros en diferentes partes del sistema.

---

## Problema sin Specification

```csharp
var activeClients = clients
    .Where(x => x.IsActive && !x.IsDeleted);
```

Ese filtro puede repetirse en muchos lugares.

---

## Solución

```csharp
public sealed class ActiveClientsSpecification
    : Specification<Client>
{
    public ActiveClientsSpecification()
    {
        AddCriteria(client => client.IsActive);
        AddCriteria(client => !client.IsDeleted);
    }
}
```

---

# ISpecification

Contrato base para representar una specification.

Puede contener:

```text
Criteria
Includes
OrderBy
OrderByDescending
Skip
Take
IsPagingEnabled
```

---

# Specification

Clase base para construir specifications.

---

## Ejemplo básico

```csharp
using CustomCodeFramework.Persistence.Specifications;

public sealed class ClientByEmailSpecification
    : Specification<Client>
{
    public ClientByEmailSpecification(string email)
    {
        AddCriteria(client => client.Email == email);
    }
}
```

---

## Uso

```csharp
var specification = new ClientByEmailSpecification(command.Email);

var existingClient = await repository.FirstOrDefaultAsync(
    specification,
    cancellationToken);
```

---

# Specification para búsqueda

```csharp
using CustomCodeFramework.Persistence.Specifications;

public sealed class SearchClientsSpecification
    : Specification<Client>
{
    public SearchClientsSpecification(
        string? search,
        int pageNumber,
        int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            AddCriteria(client =>
                client.Name.Contains(search) ||
                client.Email.Contains(search));
        }

        ApplyOrderBy(client => client.Name);

        ApplyPaging(
            skip: (pageNumber - 1) * pageSize,
            take: pageSize);
    }
}
```

---

# Specification con estado activo

```csharp
using CustomCodeFramework.Persistence.Specifications;

public sealed class ActiveClientsSpecification
    : Specification<Client>
{
    public ActiveClientsSpecification()
    {
        AddCriteria(client => client.IsActive);
    }
}
```

---

# Specification con includes

```csharp
using CustomCodeFramework.Persistence.Specifications;

public sealed class ClientWithOrdersSpecification
    : Specification<Client>
{
    public ClientWithOrdersSpecification(Guid clientId)
    {
        AddCriteria(client => client.Id == clientId);

        AddInclude(client => client.Orders);
    }
}
```

---

# TransactionScopeOptions

Define opciones para ejecutar transacciones.

Ejemplo conceptual:

```csharp
var options = new TransactionScopeOptions
{
    IsolationLevel = "ReadCommitted",
    TimeoutSeconds = 30
};
```

---

# TransactionResult

Representa el resultado de una transacción.

---

## Success

```csharp
var result = TransactionResult.Success();
```

---

## Failure

```csharp
var result = TransactionResult.Failure(
    "transaction.failed",
    "Transaction failed.");
```

---

# Ejemplo completo: Crear cliente

## Command

```csharp
public sealed record CreateClientCommand(
    string Name,
    string Email);
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
        var emailExists = await repository.AnyAsync(
            new ClientByEmailSpecification(command.Email),
            cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("Client email already exists.");
        }

        var client = new Client(
            Guid.NewGuid(),
            command.Name,
            command.Email);

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

# Ejemplo completo: Actualizar cliente

## Command

```csharp
public sealed record UpdateClientCommand(
    Guid ClientId,
    string Name,
    string Email);
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
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
            command.Email);

        repository.Update(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
```

---

# Ejemplo completo: Eliminar cliente

## Command

```csharp
public sealed record DeleteClientCommand(Guid ClientId);
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class DeleteClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteClientCommand>
{
    public async Task HandleAsync(
        DeleteClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = await repository.GetByIdAsync(
            command.ClientId,
            cancellationToken);

        if (client is null)
        {
            return;
        }

        repository.Delete(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
```

---

# Ejemplo completo: Soft delete

## Entidad

```csharp
using CustomCodeFramework.Core.Domain.Entities;

public sealed class Client : SoftDeletableEntity<Guid>
{
    public string Name { get; private set; } = default!;

    public void Delete(string userId)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        DeletedBy = userId;
    }
}
```

---

## Handler

```csharp
using CustomCodeFramework.Auth.Abstractions;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class SoftDeleteClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : ICommandHandler<DeleteClientCommand>
{
    public async Task HandleAsync(
        DeleteClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = await repository.GetByIdAsync(
            command.ClientId,
            cancellationToken);

        if (client is null)
        {
            return;
        }

        client.Delete(currentUser.UserId ?? "system");

        repository.Update(client);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
```

---

# Ejemplo completo: Validar duplicado con Specification

## Specification

```csharp
using CustomCodeFramework.Persistence.Specifications;

public sealed class ClientByEmailSpecification
    : Specification<Client>
{
    public ClientByEmailSpecification(string email)
    {
        AddCriteria(client => client.Email == email);
    }
}
```

---

## Handler

```csharp
public sealed class CreateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await repository.AnyAsync(
            new ClientByEmailSpecification(command.Email),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Client already exists.");
        }

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

# Ejemplo completo: Transacción manual

## Servicio

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class InvoiceApprovalService(
    ITransactionManager transactionManager)
{
    public Task ApproveAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        return transactionManager.ExecuteAsync(
            async ct =>
            {
                // 1. Marcar factura como aprobada.
                // 2. Crear asiento contable.
                // 3. Crear evento de integración.
                // 4. Guardar cambios.
            },
            cancellationToken);
    }
}
```

---

# Ejemplo completo: Crear orden con varias entidades

## Command

```csharp
public sealed record CreateSalesOrderCommand(
    Guid ClientId,
    IReadOnlyCollection<CreateSalesOrderLineRequest> Lines);

public sealed record CreateSalesOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice);
```

---

## Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class CreateSalesOrderCommandHandler(
    IRepository<SalesOrder, Guid> salesOrderRepository,
    IRepository<Client, Guid> clientRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSalesOrderCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateSalesOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var client = await clientRepository.GetByIdAsync(
            command.ClientId,
            cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        var salesOrder = new SalesOrder(
            Guid.NewGuid(),
            command.ClientId);

        foreach (var line in command.Lines)
        {
            salesOrder.AddLine(
                line.ProductId,
                line.Quantity,
                line.UnitPrice);
        }

        await salesOrderRepository.AddAsync(
            salesOrder,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return salesOrder.Id;
    }
}
```

---

# Ejemplo completo: Query usando IReadRepository

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class GetClientByIdQueryHandler(
    IReadRepository<Client, Guid> repository)
    : IQueryHandler<GetClientByIdQuery, ClientResponse?>
{
    public async Task<ClientResponse?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var client = await repository.GetByIdAsync(
            query.ClientId,
            cancellationToken);

        if (client is null)
        {
            return null;
        }

        return new ClientResponse(
            client.Id,
            client.Name,
            client.Email);
    }
}
```

---

# Cuándo usar IRepository

Usar `IRepository` cuando:

```text
se crea una entidad
se actualiza una entidad
se elimina una entidad
se trabaja con agregados
se deben guardar cambios
se necesita UnitOfWork
```

Ejemplos:

```text
CreateClientCommandHandler
UpdateClientCommandHandler
DeleteClientCommandHandler
ApproveOrderCommandHandler
CreateInvoiceCommandHandler
```

---

# Cuándo usar IReadRepository

Usar `IReadRepository` cuando:

```text
se consulta por ID
se consulta por specification
se necesita leer entidad del dominio
no se modifica estado
```

Ejemplos:

```text
GetClientByIdQueryHandler
GetOrderByIdQueryHandler
GetProductByIdQueryHandler
```

---

# Cuándo NO usar IRepository

No usar para:

```text
reportes grandes
dashboards
consultas con muchos joins
consultas agregadas
consultas optimizadas
read models
```

Para eso usar:

```text
CustomCodeFramework.Postgres.Dapper
CustomCodeFramework.Mongo
```

---

# Integración con CQRS

## Commands

```text
CommandHandler
    ↓
IRepository
    ↓
IUnitOfWork
```

---

## Queries

```text
QueryHandler
    ↓
IReadRepository
```

O:

```text
QueryHandler
    ↓
Dapper
```

---

# Integración con EntityFramework

El paquete `CustomCodeFramework.Postgres.EntityFramework` implementa:

```text
IRepository
IReadRepository
IUnitOfWork
ITransactionManager
```

Por lo tanto, la aplicación solo depende de:

```text
CustomCodeFramework.Persistence
```

Y la infraestructura depende de:

```text
CustomCodeFramework.Postgres.EntityFramework
```

---

# Integración con Dapper

El paquete `CustomCodeFramework.Postgres.Dapper` se usa principalmente para queries.

```text
Persistence define contratos
Dapper ejecuta consultas optimizadas
```

---

# Integración con Mongo

Mongo puede usarse para:

```text
read models
projections
consultas rápidas
documentos agregados
```

En ese caso, `Persistence` define los patrones y `Mongo` implementa repositorios/document stores.

---

# Organización recomendada

```text
Application
├── Clients
│   ├── CreateClient
│   │   ├── CreateClientCommand.cs
│   │   ├── CreateClientCommandHandler.cs
│   │   └── CreateClientCommandValidator.cs
│   │
│   ├── UpdateClient
│   │   ├── UpdateClientCommand.cs
│   │   ├── UpdateClientCommandHandler.cs
│   │   └── UpdateClientCommandValidator.cs
│   │
│   └── GetClientById
│       ├── GetClientByIdQuery.cs
│       └── GetClientByIdQueryHandler.cs
│
Domain
└── Clients
    ├── Client.cs
    ├── ClientCreatedDomainEvent.cs
    └── Specifications
        └── ClientByEmailSpecification.cs
```

---

# Buenas prácticas

## Sí

```csharp
IRepository<Client, Guid>
```

```csharp
IUnitOfWork
```

```csharp
Specification<Client>
```

```csharp
IReadRepository<Client, Guid>
```

---

## No

```csharp
AppDbContext dentro del Application
```

```csharp
DbSet directamente en handlers
```

```csharp
SQL directo en commands
```

```csharp
SaveChangesAsync en endpoint
```

---

# Errores comunes

## Usar UnitOfWork en Query

Incorrecto:

```csharp
public sealed class GetClientQueryHandler(
    IReadRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork)
{
}
```

Correcto:

```csharp
public sealed class GetClientQueryHandler(
    IReadRepository<Client, Guid> repository)
{
}
```

---

## Usar IRepository para reportes

Incorrecto:

```csharp
repository.ListAsync(new SalesReportSpecification());
```

Correcto:

```csharp
queryExecutor.QueryAsync<SalesReportRow>(sql);
```

---

## Guardar cambios varias veces

Incorrecto:

```csharp
await repository.AddAsync(client);
await unitOfWork.SaveChangesAsync();

await repository.AddAsync(order);
await unitOfWork.SaveChangesAsync();
```

Correcto:

```csharp
await repository.AddAsync(client);
await repository.AddAsync(order);

await unitOfWork.SaveChangesAsync();
```

---

# Flujo recomendado de escritura

```text
Command
    ↓
Handler
    ↓
Repository
    ↓
Aggregate
    ↓
UnitOfWork
    ↓
Domain Events
    ↓
Outbox
```

---

# Flujo recomendado de lectura

```text
Query
    ↓
Handler
    ↓
ReadRepository / Dapper / Mongo
    ↓
DTO
```

---

# Resumen

Usar `CustomCodeFramework.Persistence` para:

```text
definir contratos de persistencia
evitar dependencia directa de EF Core
mantener Application desacoplado
centralizar UnitOfWork
manejar repositorios
crear specifications reutilizables
trabajar con transacciones
```

Este paquete es una capa de abstracción. Las implementaciones reales deben vivir en paquetes de infraestructura como:

```text
CustomCodeFramework.Postgres.EntityFramework
CustomCodeFramework.Postgres.Dapper
CustomCodeFramework.Mongo
```
