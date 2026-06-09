# CustomCodeFramework.Postgres

## Introducción

`CustomCodeFramework.Postgres` es el paquete base para trabajar con PostgreSQL dentro de CustomCodeFramework.

Este paquete no implementa repositorios EF ni ejecutores Dapper directamente. Su objetivo es centralizar infraestructura común de PostgreSQL:

- Options.
- Connection string provider.
- Connection factory base.
- Health checks.
- Retry policies.
- Script runner.
- Database initializer.
- Extensiones de DI.
- Extensiones de aplicación.

Los paquetes especializados son:

```text
CustomCodeFramework.Postgres.EntityFramework
CustomCodeFramework.Postgres.Dapper
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=postgres",
    "CommandTimeoutSeconds": 30,
    "EnableRetry": true,
    "MaxRetryCount": 3,
    "MaxRetryDelaySeconds": 5
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Postgres.DependencyInjection;

builder.Services.AddCustomCodePostgres(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Postgres
├── Abstractions
│   ├── IPostgresConnectionStringProvider.cs
│   ├── IPostgresConnectionFactory.cs
│   └── IPostgresDatabaseInitializer.cs
│
├── Connections
│   ├── PostgresConnectionFactory.cs
│   └── PostgresConnectionStringProvider.cs
│
├── DependencyInjection
│   ├── PostgresServiceCollectionExtensions.cs
│   └── PostgresApplicationBuilderExtensions.cs
│
├── HealthChecks
│   ├── PostgresHealthCheck.cs
│   └── PostgresHealthCheckExtensions.cs
│
├── Options
│   ├── PostgresOptions.cs
│   └── PostgresOptionsValidator.cs
│
├── Resilience
│   ├── PostgresRetryOptions.cs
│   └── PostgresRetryPolicy.cs
│
└── Scripts
    ├── ISqlScriptRunner.cs
    └── SqlScriptRunner.cs
```

---

# IPostgresConnectionStringProvider

## ¿Para qué sirve?

Permite centralizar de dónde sale el connection string.

Evita que cada clase lea `IConfiguration` directamente.

---

## Uso

```csharp
using CustomCodeFramework.Postgres.Abstractions;

public sealed class DatabaseInfoService(
    IPostgresConnectionStringProvider connectionStringProvider)
{
    public string GetConnectionString()
    {
        return connectionStringProvider.GetConnectionString();
    }
}
```

---

# IPostgresConnectionFactory

## ¿Para qué sirve?

Crea conexiones a PostgreSQL.

Lo usan paquetes como:

```text
CustomCodeFramework.Postgres.Dapper
CustomCodeFramework.Postgres.EntityFramework
```

---

## Uso conceptual

```csharp
using CustomCodeFramework.Postgres.Abstractions;

public sealed class RawPostgresQuery(
    IPostgresConnectionFactory connectionFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(
            cancellationToken);

        // Ejecutar comandos directos si fuera necesario.
    }
}
```

---

# Health Check

## Registro

```csharp
using CustomCodeFramework.Postgres.HealthChecks;

builder.Services.AddCustomCodePostgresHealthCheck(builder.Configuration);
```

## Endpoint

```csharp
app.MapHealthChecks("/health/postgres");
```

---

# Script Runner

## ¿Para qué sirve?

Sirve para ejecutar scripts SQL de inicialización.

Ejemplos:

```text
crear extensions
crear schemas
crear outbox
crear inbox
crear funciones base
crear índices
```

---

## Ejemplo de script

```sql
create schema if not exists app;

create extension if not exists "uuid-ossp";
```

---

## Uso

```csharp
using CustomCodeFramework.Postgres.Scripts;

public sealed class DatabaseBootstrapper(
    ISqlScriptRunner scriptRunner)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await scriptRunner.RunAsync(
            """
            create schema if not exists app;
            create extension if not exists "uuid-ossp";
            """,
            cancellationToken);
    }
}
```

---

# Database Initializer

## ¿Para qué sirve?

Permite correr inicialización al iniciar la aplicación.

---

## Ejemplo

```csharp
using CustomCodeFramework.Postgres.Abstractions;

public sealed class AppDatabaseInitializer(
    ISqlScriptRunner scriptRunner)
    : IPostgresDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await scriptRunner.RunAsync(
            """
            create schema if not exists app;

            create table if not exists app.database_version (
                id uuid primary key,
                version text not null,
                applied_at_utc timestamp with time zone not null
            );
            """,
            cancellationToken);
    }
}
```

---

# Ejecutar inicializador desde Program.cs

```csharp
using CustomCodeFramework.Postgres.DependencyInjection;

var app = builder.Build();

await app.UseCustomCodePostgresInitializerAsync();

app.Run();
```

---

# Resilience / Retry

## ¿Para qué sirve?

Permite reintentar operaciones cuando PostgreSQL tiene errores transitorios.

Ejemplos:

```text
timeout temporal
conexión recién levantando
contenedor iniciando
failover
```

---

## Configuración

```json
{
  "Postgres": {
    "EnableRetry": true,
    "MaxRetryCount": 3,
    "MaxRetryDelaySeconds": 5
  }
}
```

---

# Uso recomendado

Usar `CustomCodeFramework.Postgres` cuando un servicio necesite PostgreSQL, aunque use EF o Dapper.

Ejemplo API con PostgreSQL:

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Postgres.EntityFramework --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Postgres.Dapper --version 0.1.0-alpha.1
```

---

# Program.cs básico

```csharp
using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomCodePostgres(builder.Configuration);
builder.Services.AddCustomCodePostgresHealthCheck(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health/postgres");

app.Run();
```

---

# Buenas prácticas

## Sí

```csharp
builder.Services.AddCustomCodePostgres(builder.Configuration);
```

```csharp
IPostgresConnectionStringProvider
```

```csharp
IPostgresConnectionFactory
```

```csharp
ISqlScriptRunner
```

---

## No

```csharp
configuration.GetConnectionString("Postgres")
```

en cada clase.

---

# Errores comunes

## Connection string duplicado

Incorrecto:

```json
{
  "ConnectionStrings": {
    "Default": "..."
  },
  "Postgres": {
    "ConnectionString": "..."
  }
}
```

Correcto:

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=postgres"
  }
}
```

---

## Usar Postgres base esperando repositorios

Incorrecto:

```csharp
builder.Services.AddCustomCodePostgres(builder.Configuration);

// Esperar que IRepository funcione automáticamente.
```

Correcto:

```csharp
builder.Services.AddCustomCodePostgres(builder.Configuration);
builder.Services.AddCustomCodePostgresEntityFramework<AppDbContext>(builder.Configuration);
```

---

# Resumen

`CustomCodeFramework.Postgres` es el paquete base de infraestructura PostgreSQL.

Debe usarse para:

```text
configurar PostgreSQL
crear conexiones
centralizar connection string
agregar health checks
ejecutar scripts
manejar inicialización
configurar retry policies
```

No debe contener lógica de EF Core ni Dapper directamente.

---

# CustomCodeFramework.Postgres.EntityFramework

## Introducción

`CustomCodeFramework.Postgres.EntityFramework` implementa persistencia PostgreSQL usando Entity Framework Core.

Este paquete es el encargado de implementar los contratos de:

```text
CustomCodeFramework.Persistence
```

como:

```text
IRepository
IReadRepository
IUnitOfWork
ITransactionManager
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Postgres.EntityFramework --version 0.1.0-alpha.1
```

También necesitás:

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Persistence --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=postgres",
    "CommandTimeoutSeconds": 30
  },
  "EntityFramework": {
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "UseSnakeCaseNaming": true,
    "ApplyMigrationsOnStartup": false
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;

builder.Services.AddCustomCodePostgres(builder.Configuration);

builder.Services.AddCustomCodePostgresEntityFramework<AppDbContext>(
    builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Postgres.EntityFramework
├── Abstractions
│   ├── IDbContext.cs
│   ├── IEfUnitOfWork.cs
│   └── IEfRepository.cs
│
├── Configurations
│   ├── EntityTypeConfigurationBase.cs
│   ├── AuditableEntityConfiguration.cs
│   ├── SoftDeletableEntityConfiguration.cs
│   ├── OutboxMessageConfiguration.cs
│   └── InboxMessageConfiguration.cs
│
├── Conventions
│   ├── SnakeCaseNamingConvention.cs
│   ├── DateTimeUtcConvention.cs
│   └── DecimalPrecisionConvention.cs
│
├── DbContexts
│   ├── AppDbContextBase.cs
│   ├── DbContextOptionsExtensions.cs
│   └── ModelBuilderExtensions.cs
│
├── DependencyInjection
│   ├── EntityFrameworkServiceCollectionExtensions.cs
│   └── EntityFrameworkApplicationBuilderExtensions.cs
│
├── Interceptors
│   ├── AuditableEntityInterceptor.cs
│   ├── SoftDeleteInterceptor.cs
│   ├── DomainEventInterceptor.cs
│   ├── OutboxSaveChangesInterceptor.cs
│   └── ConcurrencyInterceptor.cs
│
├── Migrations
│   ├── MigrationRunner.cs
│   └── MigrationOptions.cs
│
├── Repositories
│   ├── EfRepository.cs
│   ├── EfReadRepository.cs
│   └── SpecificationEvaluator.cs
│
├── Transactions
│   ├── EfTransactionManager.cs
│   └── EfTransactionOptions.cs
│
├── UnitOfWork
│   └── EfUnitOfWork.cs
│
├── Outbox
│   ├── EfOutboxStore.cs
│   ├── EfOutboxMessageMapper.cs
│   └── EfOutboxProcessor.cs
│
└── Inbox
    ├── EfInboxStore.cs
    ├── EfInboxMessageMapper.cs
    └── EfInboxProcessor.cs
```

---

# Crear DbContext

## AppDbContext

```csharp
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : AppDbContextBase(options)
{
    public DbSet<Client> Clients => Set<Client>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderConfiguration());
    }
}
```

---

# Entidad

```csharp
using CustomCodeFramework.Core.Domain.Entities;

public sealed class Client : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

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

        RaiseDomainEvent(
            new ClientCreatedDomainEvent(
                id,
                name,
                email));
    }

    public void Update(
        string name,
        string email)
    {
        Name = name;
        Email = email;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
```

---

# Configuración de entidad

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ClientConfiguration
    : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients", "app");

        builder.HasKey(client => client.Id);

        builder.Property(client => client.Id)
            .HasColumnName("client_id");

        builder.Property(client => client.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(client => client.Email)
            .HasColumnName("email")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(client => client.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(client => client.Email)
            .IsUnique();
    }
}
```

---

# Usar convenciones snake_case

Si el paquete tiene convención snake_case habilitada, se busca que:

```text
ClientId -> client_id
CreatedAtUtc -> created_at_utc
SalesOrderLines -> sales_order_lines
```

Configuración:

```json
{
  "EntityFramework": {
    "UseSnakeCaseNaming": true
  }
}
```

---

# Repositorio EF

## Crear cliente

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

# Actualizar cliente

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

# Eliminar cliente

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

# IReadRepository con EF

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
            client.Email,
            client.IsActive);
    }
}
```

---

# Specifications con EF

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

## Uso

```csharp
var exists = await repository.AnyAsync(
    new ClientByEmailSpecification(command.Email),
    cancellationToken);

if (exists)
{
    throw new InvalidOperationException("Client already exists.");
}
```

---

# Transacciones EF

## Handler con UnitOfWork

```csharp
public sealed class CreateSalesOrderCommandHandler(
    IRepository<SalesOrder, Guid> orderRepository,
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

        var order = new SalesOrder(
            Guid.NewGuid(),
            command.ClientId);

        foreach (var line in command.Lines)
        {
            order.AddLine(
                line.ProductId,
                line.Quantity,
                line.UnitPrice);
        }

        await orderRepository.AddAsync(
            order,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return order.Id;
    }
}
```

---

# ITransactionManager

Para casos donde se requiere control transaccional explícito.

```csharp
using CustomCodeFramework.Persistence.Abstractions;

public sealed class ApproveInvoiceService(
    ITransactionManager transactionManager)
{
    public Task ApproveAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        return transactionManager.ExecuteAsync(
            async ct =>
            {
                // 1. Aprobar factura.
                // 2. Crear asiento contable.
                // 3. Registrar evento.
                // 4. Guardar cambios.
            },
            cancellationToken);
    }
}
```

---

# Migrations

## Crear migración

Desde el proyecto donde está el DbContext:

```bash
dotnet ef migrations add InitialCreate \
  --project src/MyService.Infrastructure \
  --startup-project src/MyService.Api \
  --context AppDbContext
```

---

## Actualizar base

```bash
dotnet ef database update \
  --project src/MyService.Infrastructure \
  --startup-project src/MyService.Api \
  --context AppDbContext
```

---

## Ejecutar migraciones al iniciar

```json
{
  "EntityFramework": {
    "ApplyMigrationsOnStartup": true
  }
}
```

Program.cs:

```csharp
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;

var app = builder.Build();

await app.UseCustomCodeEntityFrameworkMigrationsAsync<AppDbContext>();

app.Run();
```

---

# AuditableEntityInterceptor

Si una entidad hereda de `AuditableEntity`, el interceptor puede llenar automáticamente:

```text
CreatedAtUtc
CreatedBy
UpdatedAtUtc
UpdatedBy
```

Ejemplo entidad:

```csharp
using CustomCodeFramework.Core.Domain.Entities;

public sealed class Client : AuditableEntity<Guid>
{
    public string Name { get; private set; } = default!;
}
```

---

# SoftDeleteInterceptor

Si una entidad hereda de `SoftDeletableEntity`, el delete puede convertirse en soft delete.

```csharp
using CustomCodeFramework.Core.Domain.Entities;

public sealed class Client : SoftDeletableEntity<Guid>
{
    public string Name { get; private set; } = default!;
}
```

Resultado esperado:

```text
delete físico -> update is_deleted = true
```

---

# DomainEventInterceptor

Permite recolectar domain events desde aggregate roots.

Flujo:

```text
AggregateRoot
    ↓
Domain Events
    ↓
SaveChanges
    ↓
Interceptor
    ↓
Outbox
```

---

# Outbox con EF

## ¿Para qué sirve?

Evita publicar eventos externos antes de confirmar la base de datos.

Problema:

```text
Guardar entidad OK
Publicar evento falla
```

O:

```text
Publicar evento OK
Guardar entidad falla
```

Solución:

```text
Guardar entidad + guardar outbox en la misma transacción
Worker publica después
```

---

## Flujo

```text
CommandHandler
    ↓
AggregateRoot.RaiseDomainEvent()
    ↓
UnitOfWork.SaveChangesAsync()
    ↓
OutboxSaveChangesInterceptor
    ↓
Insert outbox_messages
    ↓
Transaction commit
    ↓
Outbox processor
    ↓
Message broker
```

---

# Inbox con EF

## ¿Para qué sirve?

Evita procesar dos veces el mismo mensaje.

Flujo:

```text
Mensaje recibido
    ↓
Inbox check
    ↓
Si existe, ignorar
    ↓
Si no existe, procesar
    ↓
Guardar inbox message
```

---

# Program.cs completo con EF

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Persistence.DependencyInjection;
using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();
builder.Services.AddCustomCodePersistence();

builder.Services.AddCustomCodePostgres(builder.Configuration);

builder.Services.AddCustomCodePostgresEntityFramework<AppDbContext>(
    builder.Configuration);

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

# Cuándo usar EntityFramework

Usar EF para:

```text
commands
aggregates
unit of work
transacciones
domain events
outbox
inbox
persistencia principal
```

No usar EF para:

```text
reportes muy pesados
dashboards con muchos joins
queries extremadamente optimizadas
lecturas masivas
```

Para eso usar:

```text
CustomCodeFramework.Postgres.Dapper
CustomCodeFramework.Mongo
```

---

# Buenas prácticas EF

## Sí

```csharp
IRepository<Client, Guid>
IUnitOfWork
Specification<Client>
AppDbContextBase
IEntityTypeConfiguration<T>
```

---

## No

```csharp
AppDbContext en Application
DbSet directo en handlers
SaveChangesAsync en endpoint
SQL crudo en commands
```

---

# CustomCodeFramework.Postgres.Dapper

## Introducción

`CustomCodeFramework.Postgres.Dapper` implementa consultas y comandos SQL usando Dapper sobre PostgreSQL.

Este paquete se recomienda principalmente para:

```text
queries
read models
reportes
dashboards
consultas optimizadas
consultas paginadas
queries con joins
```

No reemplaza a EF para agregados de dominio.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Postgres.Dapper --version 0.1.0-alpha.1
```

También necesitás:

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=postgres",
    "CommandTimeoutSeconds": 30
  }
}
```

---

# Registro

```csharp
using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.Dapper.DependencyInjection;

builder.Services.AddCustomCodePostgres(builder.Configuration);

builder.Services.AddCustomCodePostgresDapper(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Postgres.Dapper
├── Abstractions
│   ├── ISqlConnectionFactory.cs
│   ├── ISqlQueryExecutor.cs
│   ├── ISqlCommandExecutor.cs
│   └── ISqlPaginationBuilder.cs
│
├── Connections
│   ├── NpgsqlConnectionFactory.cs
│   └── DapperConnectionFactory.cs
│
├── DependencyInjection
│   └── DapperServiceCollectionExtensions.cs
│
├── Executors
│   ├── SqlQueryExecutor.cs
│   ├── SqlCommandExecutor.cs
│   └── PaginatedSqlQueryExecutor.cs
│
├── Pagination
│   ├── SqlPagination.cs
│   ├── SqlPagedResult.cs
│   ├── SqlPaginationBuilder.cs
│   └── SqlCountQueryBuilder.cs
│
├── TypeHandlers
│   ├── DateOnlyTypeHandler.cs
│   ├── TimeOnlyTypeHandler.cs
│   ├── GuidTypeHandler.cs
│   ├── JsonbTypeHandler.cs
│   └── StronglyTypedIdTypeHandler.cs
│
├── Mapping
│   ├── DapperMappingConfiguration.cs
│   └── SnakeCaseTypeMap.cs
│
└── Transactions
    ├── DapperTransactionManager.cs
    └── DapperTransactionContext.cs
```

---

# ISqlQueryExecutor

## ¿Para qué sirve?

Ejecuta consultas SQL que devuelven datos.

---

## Query simple

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class ClientQueries(ISqlQueryExecutor queryExecutor)
{
    public Task<ClientResponse?> GetByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        return queryExecutor.QuerySingleOrDefaultAsync<ClientResponse>(
            """
            select
                client_id as ClientId,
                name as Name,
                email as Email,
                is_active as IsActive
            from app.clients
            where client_id = @ClientId
            """,
            new
            {
                ClientId = clientId
            },
            cancellationToken);
    }
}
```

---

# Query lista

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class ClientQueries(ISqlQueryExecutor queryExecutor)
{
    public Task<IReadOnlyCollection<ClientResponse>> GetActiveClientsAsync(
        CancellationToken cancellationToken)
    {
        return queryExecutor.QueryAsync<ClientResponse>(
            """
            select
                client_id as ClientId,
                name as Name,
                email as Email,
                is_active as IsActive
            from app.clients
            where is_active = true
            order by name
            """,
            cancellationToken: cancellationToken);
    }
}
```

---

# ISqlCommandExecutor

## ¿Para qué sirve?

Ejecuta comandos SQL que modifican datos.

Aunque Dapper se recomienda para lectura, también puede usarse para comandos puntuales.

---

## Insert

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class ClientSqlCommands(ISqlCommandExecutor commandExecutor)
{
    public Task<int> InsertAsync(
        Guid clientId,
        string name,
        string email,
        CancellationToken cancellationToken)
    {
        return commandExecutor.ExecuteAsync(
            """
            insert into app.clients (
                client_id,
                name,
                email,
                is_active
            )
            values (
                @ClientId,
                @Name,
                @Email,
                true
            )
            """,
            new
            {
                ClientId = clientId,
                Name = name,
                Email = email
            },
            cancellationToken);
    }
}
```

---

## Update

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class ClientSqlCommands(ISqlCommandExecutor commandExecutor)
{
    public Task<int> UpdateAsync(
        Guid clientId,
        string name,
        string email,
        CancellationToken cancellationToken)
    {
        return commandExecutor.ExecuteAsync(
            """
            update app.clients
            set
                name = @Name,
                email = @Email,
                updated_at_utc = now()
            where client_id = @ClientId
            """,
            new
            {
                ClientId = clientId,
                Name = name,
                Email = email
            },
            cancellationToken);
    }
}
```

---

## Delete

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class ClientSqlCommands(ISqlCommandExecutor commandExecutor)
{
    public Task<int> DeleteAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        return commandExecutor.ExecuteAsync(
            """
            delete from app.clients
            where client_id = @ClientId
            """,
            new
            {
                ClientId = clientId
            },
            cancellationToken);
    }
}
```

---

# Query Handler con Dapper

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
                is_active as IsActive
            from app.clients
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

# Query paginada

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
                is_active as IsActive
            from app.clients
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
            from app.clients
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

# Reporte con Dapper

## DTO

```csharp
public sealed record SalesReportRow(
    string InvoiceNumber,
    string ClientName,
    DateTime IssueDate,
    decimal Subtotal,
    decimal Taxes,
    decimal Total);
```

---

## Query service

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class SalesReportQueryService(
    ISqlQueryExecutor queryExecutor)
{
    public Task<IReadOnlyCollection<SalesReportRow>> GetAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return queryExecutor.QueryAsync<SalesReportRow>(
            """
            select
                i.invoice_number as InvoiceNumber,
                c.name as ClientName,
                i.issue_date as IssueDate,
                i.subtotal as Subtotal,
                i.taxes as Taxes,
                i.total as Total
            from app.invoices i
            inner join app.clients c on c.client_id = i.client_id
            where i.issue_date >= @From
              and i.issue_date < @To
            order by i.issue_date desc
            """,
            new
            {
                From = from,
                To = to
            },
            cancellationToken);
    }
}
```

---

# Dashboard con Dapper

```csharp
public sealed record DashboardMetrics(
    int TotalClients,
    int TotalInvoices,
    decimal TotalSales);
```

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class DashboardQueries(ISqlQueryExecutor queryExecutor)
{
    public Task<DashboardMetrics> GetMetricsAsync(
        CancellationToken cancellationToken)
    {
        return queryExecutor.QuerySingleAsync<DashboardMetrics>(
            """
            select
                (select count(*) from app.clients) as TotalClients,
                (select count(*) from app.invoices) as TotalInvoices,
                (select coalesce(sum(total), 0) from app.invoices) as TotalSales
            """,
            cancellationToken: cancellationToken);
    }
}
```

---

# JSONB

## Tabla ejemplo

```sql
create table app.integration_events (
    integration_event_id uuid primary key,
    event_name text not null,
    payload jsonb not null,
    created_at_utc timestamp with time zone not null
);
```

---

## DTO

```csharp
public sealed record IntegrationEventReadModel(
    Guid IntegrationEventId,
    string EventName,
    object Payload,
    DateTime CreatedAtUtc);
```

---

## Query JSONB

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;

public sealed class IntegrationEventQueries(
    ISqlQueryExecutor queryExecutor)
{
    public Task<IReadOnlyCollection<IntegrationEventReadModel>> GetAsync(
        string eventName,
        CancellationToken cancellationToken)
    {
        return queryExecutor.QueryAsync<IntegrationEventReadModel>(
            """
            select
                integration_event_id as IntegrationEventId,
                event_name as EventName,
                payload as Payload,
                created_at_utc as CreatedAtUtc
            from app.integration_events
            where event_name = @EventName
            order by created_at_utc desc
            """,
            new
            {
                EventName = eventName
            },
            cancellationToken);
    }
}
```

---

# DateOnly y TimeOnly

El paquete incluye type handlers para:

```text
DateOnly
TimeOnly
Guid
Jsonb
StronglyTypedId
```

Ejemplo:

```csharp
public sealed record AppointmentResponse(
    Guid AppointmentId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime);
```

Query:

```csharp
return queryExecutor.QueryAsync<AppointmentResponse>(
    """
    select
        appointment_id as AppointmentId,
        appointment_date as Date,
        start_time as StartTime,
        end_time as EndTime
    from app.appointments
    """,
    cancellationToken: cancellationToken);
```

---

# Transacción con Dapper

```csharp
using CustomCodeFramework.Postgres.Dapper.Transactions;

public sealed class ClientImportService(
    DapperTransactionManager transactionManager)
{
    public Task ImportAsync(
        IReadOnlyCollection<ClientImportRow> clients,
        CancellationToken cancellationToken)
    {
        return transactionManager.ExecuteAsync(
            async transaction =>
            {
                foreach (var client in clients)
                {
                    await transaction.ExecuteAsync(
                        """
                        insert into app.clients (
                            client_id,
                            name,
                            email
                        )
                        values (
                            @ClientId,
                            @Name,
                            @Email
                        )
                        """,
                        new
                        {
                            ClientId = Guid.NewGuid(),
                            client.Name,
                            client.Email
                        });
                }
            },
            cancellationToken);
    }
}
```

---

# Usar Dapper para Read Models

```csharp
public sealed record ClientListItem(
    Guid ClientId,
    string Name,
    string Email,
    int TotalOrders,
    decimal TotalSales);
```

```csharp
public sealed class ClientListQueries(ISqlQueryExecutor queryExecutor)
{
    public Task<IReadOnlyCollection<ClientListItem>> GetAsync(
        CancellationToken cancellationToken)
    {
        return queryExecutor.QueryAsync<ClientListItem>(
            """
            select
                c.client_id as ClientId,
                c.name as Name,
                c.email as Email,
                count(o.sales_order_id) as TotalOrders,
                coalesce(sum(o.total), 0) as TotalSales
            from app.clients c
            left join app.sales_orders o on o.client_id = c.client_id
            group by c.client_id, c.name, c.email
            order by c.name
            """,
            cancellationToken: cancellationToken);
    }
}
```

---

# Program.cs con EF + Dapper

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Persistence.DependencyInjection;
using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.Dapper.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();
builder.Services.AddCustomCodePersistence();

builder.Services.AddCustomCodePostgres(builder.Configuration);

builder.Services.AddCustomCodePostgresEntityFramework<AppDbContext>(
    builder.Configuration);

builder.Services.AddCustomCodePostgresDapper(builder.Configuration);

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

# Cuándo usar Dapper

Usar Dapper para:

```text
queries
reportes
dashboards
joins complejos
read models
consultas paginadas
consultas optimizadas
consultas con SQL manual
```

---

# Cuándo NO usar Dapper

No usar Dapper como primera opción para:

```text
agregados complejos
domain events
unit of work de dominio
outbox automático
tracking de entidades
relaciones complejas de escritura
```

Para eso usar:

```text
CustomCodeFramework.Postgres.EntityFramework
```

---

# EF vs Dapper

## Usar EF para escritura

```text
Commands
Aggregates
Repositories
UnitOfWork
Domain Events
Outbox
Inbox
```

---

## Usar Dapper para lectura

```text
Queries
Reports
Dashboards
Read Models
Search
Pagination
```

---

# Flujo recomendado

## Escritura

```text
Command
    ↓
CommandHandler
    ↓
IRepository
    ↓
EF Core
    ↓
UnitOfWork
    ↓
Outbox
```

---

## Lectura

```text
Query
    ↓
QueryHandler
    ↓
Dapper
    ↓
DTO
```

---

# Buenas prácticas

## Sí

```csharp
ISqlQueryExecutor
```

```csharp
ISqlCommandExecutor
```

```csharp
DTOs específicos para lectura
```

```csharp
SQL claro y explícito
```

```csharp
Parámetros anónimos
```

---

## No

```csharp
SQL concatenado con strings del usuario
```

```csharp
select *
```

```csharp
Dapper para agregados complejos
```

```csharp
EF para reportes gigantes
```

---

# Errores comunes

## SQL injection

Incorrecto:

```csharp
var sql = $"select * from clients where email = '{email}'";
```

Correcto:

```csharp
var sql = """
select *
from clients
where email = @Email
""";

await queryExecutor.QueryAsync<ClientResponse>(
    sql,
    new
    {
        Email = email
    },
    cancellationToken);
```

---

## Usar select \*

Incorrecto:

```sql
select *
from clients
```

Correcto:

```sql
select
    client_id as ClientId,
    name as Name,
    email as Email
from clients
```

---

## Mezclar escritura EF y Dapper sin transacción

Incorrecto:

```csharp
await dbContext.SaveChangesAsync();
await commandExecutor.ExecuteAsync(sql);
```

Correcto:

```text
usar transaction manager
o mantener la escritura principal en EF
```

---

# Resumen

## CustomCodeFramework.Postgres

Usar para:

```text
configuración base
connection string
connection factory
health checks
scripts SQL
retry policies
inicialización
```

## CustomCodeFramework.Postgres.EntityFramework

Usar para:

```text
commands
aggregates
repositories
unit of work
transactions
domain events
outbox
inbox
migrations
```

## CustomCodeFramework.Postgres.Dapper

Usar para:

```text
queries
read models
reportes
dashboards
consultas SQL optimizadas
paginación
joins complejos
```

Combinación recomendada:

```text
EF Core para escritura
Dapper para lectura
Postgres base para infraestructura común
```
