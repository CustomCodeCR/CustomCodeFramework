# CustomCodeFramework.Redis

## Introducción

`CustomCodeFramework.Redis` es el paquete encargado de integrar Redis dentro de CustomCodeFramework.

Este paquete se usa principalmente para:

- Cache distribuido.
- Cache-aside pattern.
- Locks distribuidos.
- Control de concurrencia.
- Guardar datos temporales.
- Optimizar queries.
- Evitar procesos duplicados.
- Guardar estados temporales de corta duración.

Redis no debe usarse como base de datos principal. Debe usarse como almacenamiento rápido y temporal.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Redis --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "customcode:",
    "DefaultExpirationMinutes": 30,
    "LockExpirationSeconds": 30
  }
}
```

Con password:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,password=CHANGE_ME",
    "InstanceName": "customcode:",
    "DefaultExpirationMinutes": 30,
    "LockExpirationSeconds": 30
  }
}
```

Con Docker:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "customcode:",
    "DefaultExpirationMinutes": 30,
    "LockExpirationSeconds": 30
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Redis.DependencyInjection;

builder.Services.AddCustomCodeRedis(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Redis
├── Options
│   └── RedisOptions.cs
│
├── Abstractions
│   ├── ICacheService.cs
│   ├── ICacheKeyBuilder.cs
│   └── IDistributedLock.cs
│
├── Caching
│   ├── RedisCacheService.cs
│   ├── CacheEntryOptions.cs
│   └── CacheKeyBuilder.cs
│
├── Locks
│   ├── RedisDistributedLock.cs
│   └── DistributedLockHandle.cs
│
├── Serialization
│   ├── ICacheSerializer.cs
│   └── SystemTextJsonCacheSerializer.cs
│
├── HealthChecks
│   └── RedisHealthCheck.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# ¿Cuándo usar Redis?

Usar Redis para:

```text
cache de queries
cache de catálogos
cache de permisos
cache de configuración
locks distribuidos
evitar trabajos duplicados
tokens temporales
sesiones temporales
rate limiting
datos calculados temporalmente
```

Ejemplos:

```text
ClientResponse cache
ProductCatalog cache
UserPermissions cache
ExchangeRate cache
DashboardMetrics cache
ReportJob lock
InvoiceGeneration lock
```

---

# ¿Cuándo NO usar Redis?

No usar Redis como almacenamiento principal para:

```text
facturas
órdenes
clientes
contabilidad
auditoría permanente
datos críticos
historial legal
```

Redis puede perder datos si no está configurado con persistencia adecuada. Debe tratarse como cache o almacenamiento temporal.

---

# RedisOptions

Configuración base:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "customcode:",
    "DefaultExpirationMinutes": 30,
    "LockExpirationSeconds": 30
  }
}
```

---

# ICacheService

## ¿Qué es?

`ICacheService` es la abstracción principal para guardar y leer valores en Redis.

Permite:

```text
Get
Set
Remove
Exists
```

---

## Guardar valor

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class ClientCache(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder)
{
    public Task SetAsync(
        Guid clientId,
        ClientResponse client,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "clients",
            clientId.ToString());

        return cacheService.SetAsync(
            key,
            client,
            cancellationToken: cancellationToken);
    }
}
```

---

## Obtener valor

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class ClientCache(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder)
{
    public Task<ClientResponse?> GetAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "clients",
            clientId.ToString());

        return cacheService.GetAsync<ClientResponse>(
            key,
            cancellationToken);
    }
}
```

---

## Eliminar valor

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class ClientCache(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder)
{
    public Task RemoveAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "clients",
            clientId.ToString());

        return cacheService.RemoveAsync(
            key,
            cancellationToken);
    }
}
```

---

# ICacheKeyBuilder

## ¿Para qué sirve?

Sirve para crear llaves consistentes.

Evita tener strings duplicados en todo el sistema.

Incorrecto:

```csharp
var key = $"client-{clientId}";
```

Correcto:

```csharp
var key = keyBuilder.Build("clients", clientId.ToString());
```

---

## Ejemplos de keys

```csharp
var clientKey = keyBuilder.Build(
    "clients",
    clientId.ToString());
```

Resultado:

```text
customcode:clients:{clientId}
```

---

```csharp
var permissionsKey = keyBuilder.Build(
    "users",
    userId,
    "permissions");
```

Resultado:

```text
customcode:users:{userId}:permissions
```

---

```csharp
var dashboardKey = keyBuilder.Build(
    "dashboard",
    "sales",
    date.ToString("yyyy-MM-dd"));
```

Resultado:

```text
customcode:dashboard:sales:2026-06-09
```

---

# CacheEntryOptions

## ¿Para qué sirve?

Permite configurar expiración por entrada.

Ejemplo:

```csharp
using CustomCodeFramework.Redis.Caching;

await cacheService.SetAsync(
    key,
    client,
    new CacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    },
    cancellationToken);
```

---

# Expiración recomendada

## Datos poco cambiantes

```text
catálogos
países
monedas
permisos
configuración general
```

Tiempo recomendado:

```csharp
TimeSpan.FromHours(1)
```

---

## Datos cambiantes

```text
clientes
órdenes
inventario
dashboards
cotizaciones
```

Tiempo recomendado:

```csharp
TimeSpan.FromMinutes(5)
```

---

## Datos críticos

```text
stock
saldo
estado de factura
```

Tiempo recomendado:

```csharp
TimeSpan.FromSeconds(30)
```

O no cachear si debe ser exacto.

---

# Cache Aside Pattern

## ¿Qué es?

Es el patrón recomendado para usar Redis como cache.

Flujo:

```text
Query
  ↓
Buscar en Redis
  ↓
Si existe -> devolver
  ↓
Si no existe -> consultar base de datos
  ↓
Guardar en Redis
  ↓
Devolver
```

---

## Ejemplo real: Cliente por ID

```csharp
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Postgres.Dapper.Abstractions;
using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;

public sealed class GetClientByIdQueryHandler(
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
                is_active as IsActive
            from app.clients
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
                new CacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                },
                cancellationToken);
        }

        return client;
    }
}
```

---

# Invalidación de cache

Cuando se actualiza una entidad, se debe eliminar su cache.

---

## Ejemplo: UpdateClientCommandHandler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Redis.Abstractions;

public sealed class UpdateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder)
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cacheKey = keyBuilder.Build(
            "clients",
            command.ClientId.ToString());

        await cacheService.RemoveAsync(
            cacheKey,
            cancellationToken);
    }
}
```

---

# Cache de listas

## Problema

Las listas cambian más seguido que un registro individual.

Ejemplo:

```text
GET /clients?pageNumber=1&pageSize=20&search=abc
```

Debe tener una key que incluya los filtros.

---

## Ejemplo

```csharp
var key = keyBuilder.Build(
    "clients",
    "search",
    query.Search ?? "all",
    query.PageNumber.ToString(),
    query.PageSize.ToString());
```

Resultado:

```text
customcode:clients:search:abc:1:20
```

---

## Handler paginado con cache

```csharp
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using CustomCodeFramework.Postgres.Dapper.Abstractions;
using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;

public sealed class SearchClientsQueryHandler(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder,
    ISqlQueryExecutor queryExecutor)
    : IQueryHandler<SearchClientsQuery, PagedResult<ClientResponse>>
{
    public async Task<PagedResult<ClientResponse>> HandleAsync(
        SearchClientsQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = keyBuilder.Build(
            "clients",
            "search",
            query.Search ?? "all",
            query.PageNumber.ToString(),
            query.PageSize.ToString());

        var cached = await cacheService.GetAsync<PagedResult<ClientResponse>>(
            cacheKey,
            cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

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

        var result = new PagedResult<ClientResponse>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        await cacheService.SetAsync(
            cacheKey,
            result,
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            },
            cancellationToken);

        return result;
    }
}
```

---

# Cache de permisos

## Caso de uso

Los permisos se consultan constantemente.

Es recomendable cachearlos.

---

## Key

```csharp
var key = keyBuilder.Build(
    "users",
    userId,
    "permissions");
```

---

## Servicio

```csharp
using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;

public sealed class UserPermissionCache(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder)
{
    public Task<IReadOnlyCollection<string>?> GetAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "users",
            userId,
            "permissions");

        return cacheService.GetAsync<IReadOnlyCollection<string>>(
            key,
            cancellationToken);
    }

    public Task SetAsync(
        string userId,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "users",
            userId,
            "permissions");

        return cacheService.SetAsync(
            key,
            permissions,
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            },
            cancellationToken);
    }

    public Task RemoveAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "users",
            userId,
            "permissions");

        return cacheService.RemoveAsync(
            key,
            cancellationToken);
    }
}
```

---

# Cache de configuración

## Ejemplo

```csharp
public sealed record AppSettingsCacheModel(
    string Country,
    string Currency,
    bool EnableNotifications,
    bool EnableReports);
```

```csharp
using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;

public sealed class AppSettingsCache(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder)
{
    public Task<AppSettingsCacheModel?> GetAsync(
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "settings",
            "app");

        return cacheService.GetAsync<AppSettingsCacheModel>(
            key,
            cancellationToken);
    }

    public Task SetAsync(
        AppSettingsCacheModel settings,
        CancellationToken cancellationToken)
    {
        var key = keyBuilder.Build(
            "settings",
            "app");

        return cacheService.SetAsync(
            key,
            settings,
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            },
            cancellationToken);
    }
}
```

---

# Distributed Lock

## ¿Qué es?

Un lock distribuido evita que dos procesos ejecuten la misma operación al mismo tiempo.

Es útil cuando hay múltiples instancias de la aplicación.

---

# ¿Cuándo usar Distributed Lock?

Usar para:

```text
procesar reportes
generar facturas
procesar outbox
ejecutar jobs recurrentes
importar archivos
sincronizar datos
liquidar pagos
crear consecutivos
```

---

# IDistributedLock

## Uso básico

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class ReportJob(
    IDistributedLock distributedLock)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await using var handle = await distributedLock.AcquireAsync(
            "locks:reports:daily-sales",
            TimeSpan.FromMinutes(5),
            cancellationToken);

        if (handle is null)
        {
            return;
        }

        // Ejecutar job.
    }
}
```

---

# Ejemplo real: Evitar doble generación de factura

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Redis.Abstractions;

public sealed class GenerateInvoiceCommandHandler(
    IDistributedLock distributedLock,
    IRepository<SalesOrder, Guid> salesOrderRepository,
    IRepository<Invoice, Guid> invoiceRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GenerateInvoiceCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        GenerateInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"locks:invoice-generation:{command.SalesOrderId}";

        await using var handle = await distributedLock.AcquireAsync(
            lockKey,
            TimeSpan.FromMinutes(2),
            cancellationToken);

        if (handle is null)
        {
            throw new InvalidOperationException(
                "Invoice generation is already in progress.");
        }

        var salesOrder = await salesOrderRepository.GetByIdAsync(
            command.SalesOrderId,
            cancellationToken);

        if (salesOrder is null)
        {
            throw new InvalidOperationException("Sales order not found.");
        }

        if (salesOrder.HasInvoice)
        {
            throw new InvalidOperationException("Sales order already has invoice.");
        }

        var invoice = Invoice.CreateFromSalesOrder(salesOrder);

        await invoiceRepository.AddAsync(
            invoice,
            cancellationToken);

        salesOrder.MarkAsInvoiced(invoice.Id);

        salesOrderRepository.Update(salesOrder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }
}
```

---

# Ejemplo real: Procesar Outbox con lock

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

# Ejemplo real: Importar archivo una sola vez

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class FileImportService(
    IDistributedLock distributedLock)
{
    public async Task ImportAsync(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var lockKey = $"locks:file-import:{fileId}";

        await using var handle = await distributedLock.AcquireAsync(
            lockKey,
            TimeSpan.FromMinutes(10),
            cancellationToken);

        if (handle is null)
        {
            throw new InvalidOperationException(
                "File is already being imported.");
        }

        // Procesar archivo.
    }
}
```

---

# DistributedLockHandle

El lock devuelve un handle disposable.

Cuando termina el `await using`, el lock se libera.

```csharp
await using var handle = await distributedLock.AcquireAsync(
    key,
    expiration,
    cancellationToken);

if (handle is null)
{
    return;
}

// trabajo protegido

// al salir del scope se libera el lock
```

---

# Serializer

## ICacheSerializer

Abstracción para serializar y deserializar objetos.

Implementación por defecto:

```text
SystemTextJsonCacheSerializer
```

---

## Ejemplo

```csharp
using CustomCodeFramework.Redis.Serialization;

public sealed class CacheSerializationExample(
    ICacheSerializer serializer)
{
    public byte[] Serialize(ClientResponse client)
    {
        return serializer.Serialize(client);
    }

    public ClientResponse? Deserialize(byte[] payload)
    {
        return serializer.Deserialize<ClientResponse>(payload);
    }
}
```

---

# SystemTextJsonCacheSerializer

Usa `System.Text.Json`.

Recomendación:

- DTOs simples.
- Records.
- Propiedades públicas.
- Evitar ciclos.
- Evitar entidades EF con navegación.

Correcto:

```csharp
public sealed record ClientResponse(
    Guid ClientId,
    string Name,
    string Email);
```

Incorrecto:

```csharp
public sealed class Client
{
    public ICollection<Order> Orders { get; set; }
}
```

---

# Health Check

## Registro

```csharp
using CustomCodeFramework.Redis.DependencyInjection;

builder.Services.AddCustomCodeRedis(builder.Configuration);
builder.Services.AddCustomCodeRedisHealthChecks(builder.Configuration);
```

---

## Endpoint

```csharp
app.MapHealthChecks("/health/redis");
```

---

# Program.cs completo

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();

builder.Services.AddCustomCodeRedis(builder.Configuration);

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

app.MapHealthChecks("/health/redis");

app.Run();
```

---

# Docker Compose

## Redis básico

```yaml
services:
  redis:
    image: redis:7
    container_name: customcode-redis
    restart: unless-stopped
    ports:
      - "6379:6379"
```

---

## Redis con password

```yaml
services:
  redis:
    image: redis:7
    container_name: customcode-redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    command: redis-server --requirepass redis-password
```

Connection string:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,password=redis-password",
    "InstanceName": "customcode:",
    "DefaultExpirationMinutes": 30,
    "LockExpirationSeconds": 30
  }
}
```

---

## Redis con persistencia

```yaml
services:
  redis:
    image: redis:7
    container_name: customcode-redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    command: redis-server --appendonly yes
    volumes:
      - redis_data:/data

volumes:
  redis_data:
```

---

# Integración con CQRS

## Query con cache

```text
QueryHandler
    ↓
ICacheService.GetAsync
    ↓
si existe devuelve cache
    ↓
si no existe consulta DB
    ↓
ICacheService.SetAsync
```

---

## Command con invalidación

```text
CommandHandler
    ↓
actualiza DB
    ↓
UnitOfWork.SaveChangesAsync
    ↓
ICacheService.RemoveAsync
```

---

# Integración con Dapper

Recomendado para cachear queries Dapper:

```text
Dapper Query
    ↓
DTO
    ↓
Redis Cache
```

Ejemplo:

```csharp
var cached = await cacheService.GetAsync<ClientResponse>(
    cacheKey,
    cancellationToken);

if (cached is not null)
{
    return cached;
}

var client = await queryExecutor.QuerySingleOrDefaultAsync<ClientResponse>(
    sql,
    parameters,
    cancellationToken);

await cacheService.SetAsync(
    cacheKey,
    client,
    TimeSpan.FromMinutes(10),
    cancellationToken);
```

---

# Integración con Auth

Redis puede cachear permisos:

```text
UserId
    ↓
permissions cache
    ↓
authorization
```

Key recomendada:

```csharp
var key = keyBuilder.Build(
    "auth",
    "users",
    userId,
    "permissions");
```

---

# Integración con Reports

Redis puede evitar doble generación de reportes.

```csharp
var lockKey = $"locks:reports:{reportKey}:{requestedBy}";

await using var handle = await distributedLock.AcquireAsync(
    lockKey,
    TimeSpan.FromMinutes(5),
    cancellationToken);

if (handle is null)
{
    throw new InvalidOperationException(
        "Report is already being generated.");
}
```

---

# Integración con Storage

Redis puede cachear URLs firmadas temporalmente.

```csharp
var cacheKey = keyBuilder.Build(
    "storage",
    "signed-url",
    filePath);

var cachedUrl = await cacheService.GetAsync<string>(
    cacheKey,
    cancellationToken);

if (!string.IsNullOrWhiteSpace(cachedUrl))
{
    return cachedUrl;
}

var signedUrl = await storageProvider.GenerateSignedUrlAsync(
    filePath,
    options,
    cancellationToken);

await cacheService.SetAsync(
    cacheKey,
    signedUrl,
    TimeSpan.FromMinutes(5),
    cancellationToken);

return signedUrl;
```

---

# Organización recomendada

```text
Infrastructure
└── Redis
    ├── CacheKeys
    │   ├── ClientCacheKeys.cs
    │   ├── UserPermissionCacheKeys.cs
    │   └── ReportCacheKeys.cs
    │
    ├── Services
    │   ├── ClientCacheService.cs
    │   ├── UserPermissionCacheService.cs
    │   └── DashboardCacheService.cs
    │
    └── Locks
        ├── ReportLockService.cs
        ├── InvoiceGenerationLockService.cs
        └── OutboxWorkerLockService.cs
```

---

# Ejemplo de CacheKeys

```csharp
public static class ClientCacheKeys
{
    public static string ById(
        ICacheKeyBuilder keyBuilder,
        Guid clientId)
    {
        return keyBuilder.Build(
            "clients",
            clientId.ToString());
    }

    public static string Search(
        ICacheKeyBuilder keyBuilder,
        string? search,
        int pageNumber,
        int pageSize)
    {
        return keyBuilder.Build(
            "clients",
            "search",
            search ?? "all",
            pageNumber.ToString(),
            pageSize.ToString());
    }
}
```

Uso:

```csharp
var key = ClientCacheKeys.ById(
    keyBuilder,
    clientId);
```

---

# Buenas prácticas

## Sí

Usar keys consistentes:

```csharp
keyBuilder.Build("clients", clientId.ToString())
```

Usar expiraciones:

```csharp
TimeSpan.FromMinutes(10)
```

Invalidar cache después de modificar datos:

```csharp
await cacheService.RemoveAsync(key, cancellationToken);
```

Usar locks para trabajos críticos:

```csharp
await distributedLock.AcquireAsync(key, expiration, cancellationToken);
```

Cachear DTOs, no entidades:

```csharp
ClientResponse
```

---

## No

No guardar entidades EF completas:

```csharp
Client
```

No guardar datos críticos sin expiración.

No usar Redis como base principal.

No crear keys a mano en todos lados.

No cachear información sensible sin protección.

No dejar locks sin expiración.

---

# Errores comunes

## Cache sin invalidación

Problema:

```text
actualizas cliente
cache sigue viejo
usuario ve datos incorrectos
```

Solución:

```csharp
await cacheService.RemoveAsync(
    key,
    cancellationToken);
```

---

## Cachear entidades con navegación

Incorrecto:

```csharp
await cacheService.SetAsync(
    "client",
    clientEntity);
```

Correcto:

```csharp
await cacheService.SetAsync(
    "client",
    clientResponse);
```

---

## Locks demasiado largos

Incorrecto:

```csharp
TimeSpan.FromHours(1)
```

Correcto:

```csharp
TimeSpan.FromMinutes(2)
```

Depende del proceso.

---

## No validar handle null

Incorrecto:

```csharp
await using var handle = await distributedLock.AcquireAsync(...);

// ejecutar aunque no tenga lock
```

Correcto:

```csharp
await using var handle = await distributedLock.AcquireAsync(...);

if (handle is null)
{
    return;
}
```

---

## Keys inconsistentes

Incorrecto:

```csharp
"client:" + clientId
"clients:" + clientId
"customcode-client-" + clientId
```

Correcto:

```csharp
keyBuilder.Build(
    "clients",
    clientId.ToString());
```

---

# Flujo recomendado de cache

## Lectura

```text
Query
    ↓
Redis Get
    ↓
Cache hit
    ↓
Return
```

Si no existe:

```text
Query
    ↓
Redis Get
    ↓
Cache miss
    ↓
Database
    ↓
Redis Set
    ↓
Return
```

---

## Escritura

```text
Command
    ↓
Update database
    ↓
Commit
    ↓
Remove cache
```

---

# Flujo recomendado de lock

```text
Operation
    ↓
Acquire lock
    ↓
If null -> another process is running
    ↓
Execute protected operation
    ↓
Dispose lock
```

---

# Resumen

`CustomCodeFramework.Redis` debe usarse para:

```text
cache distribuido
cache-aside pattern
locks distribuidos
evitar trabajos duplicados
cache de permisos
cache de catálogos
cache de configuración
optimizar queries
```

No debe usarse como base de datos principal.

Combinación recomendada:

```text
PostgreSQL -> datos principales
MongoDB -> read models
Dapper -> queries optimizadas
Redis -> cache y locks
```
