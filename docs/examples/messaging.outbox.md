# CustomCodeFramework.Messaging.Outbox

## Introducción

`CustomCodeFramework.Messaging.Outbox` proporciona una infraestructura estándar para procesar mensajes Outbox e Inbox dentro de microservicios.

Su objetivo es evitar que cada servicio implemente su propia lógica de:

- Procesamiento de Outbox.
- Limpieza de Inbox.
- Publicación confiable de eventos.
- Reintentos.
- Locks distribuidos.
- Polling periódico.
- Manejo de errores.
- Logging.
- Observabilidad.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Messaging.Outbox --version 0.1.0-alpha.1
```

---

# Estructura

```text
CustomCodeFramework.Messaging.Outbox
├── BackgroundServices
│   ├── OutboxBackgroundService.cs
│   └── InboxCleanupBackgroundService.cs
│
├── Processing
│   ├── IOutboxProcessor.cs
│   ├── IInboxProcessor.cs
│   ├── OutboxProcessingResult.cs
│   └── InboxProcessingResult.cs
│
├── Options
│   ├── OutboxBackgroundServiceOptions.cs
│   └── InboxCleanupOptions.cs
│
├── Locks
│   └── OutboxWorkerLockKeys.cs
│
└── DependencyInjection
    └── MessagingOutboxServiceCollectionExtensions.cs
```

---

# IOutboxProcessor

Contrato principal para procesar mensajes pendientes del Outbox.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Processing;

public interface IOutboxProcessor
{
    Task<OutboxProcessingResult> ProcessAsync(
        int batchSize,
        CancellationToken cancellationToken);
}
```

---

# IInboxProcessor

Contrato principal para limpiar mensajes antiguos del Inbox.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Processing;

public interface IInboxProcessor
{
    Task<InboxProcessingResult> CleanupAsync(
        TimeSpan olderThan,
        int batchSize,
        CancellationToken cancellationToken);
}
```

---

# OutboxProcessingResult

Resultado de una ejecución del procesador Outbox.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Processing;

public sealed record OutboxProcessingResult(
    int ProcessedCount,
    int FailedCount,
    bool HasMoreMessages)
{
    public static OutboxProcessingResult Empty { get; } = new(
        ProcessedCount: 0,
        FailedCount: 0,
        HasMoreMessages: false);
}
```

---

# InboxProcessingResult

Resultado de una ejecución de limpieza del Inbox.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Processing;

public sealed record InboxProcessingResult(
    int DeletedCount,
    bool HasMoreMessages)
{
    public static InboxProcessingResult Empty { get; } = new(
        DeletedCount: 0,
        HasMoreMessages: false);
}
```

---

# OutboxBackgroundServiceOptions

Configuración del worker de Outbox.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Options;

public sealed class OutboxBackgroundServiceOptions
{
    public bool Enabled { get; set; } = true;

    public int BatchSize { get; set; } = 50;

    public int PollingIntervalSeconds { get; set; } = 10;

    public bool RunImmediately { get; set; } = true;

    public bool UseDistributedLock { get; set; } = true;

    public int LockDurationSeconds { get; set; } = 60;

    public int ErrorDelaySeconds { get; set; } = 10;
}
```

---

# InboxCleanupOptions

Configuración del worker de limpieza de Inbox.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Options;

public sealed class InboxCleanupOptions
{
    public bool Enabled { get; set; } = true;

    public int BatchSize { get; set; } = 100;

    public int PollingIntervalSeconds { get; set; } = 3600;

    public int DeleteMessagesOlderThanDays { get; set; } = 30;

    public bool RunImmediately { get; set; }

    public bool UseDistributedLock { get; set; } = true;

    public int LockDurationSeconds { get; set; } = 300;

    public int ErrorDelaySeconds { get; set; } = 30;
}
```

---

# OutboxWorkerLockKeys

Llaves estándar para locks distribuidos.

```csharp
namespace CustomCodeFramework.Messaging.Outbox.Locks;

public static class OutboxWorkerLockKeys
{
    public const string OutboxProcessor = "workers:messaging:outbox-processor";

    public const string InboxCleanup = "workers:messaging:inbox-cleanup";

    public static string ForServiceOutbox(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        return $"workers:{serviceName}:messaging:outbox-processor";
    }

    public static string ForServiceInboxCleanup(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        return $"workers:{serviceName}:messaging:inbox-cleanup";
    }
}
```

---

# OutboxBackgroundService

Background service encargado de ejecutar `IOutboxProcessor`.

```csharp
using CustomCodeFramework.Messaging.Outbox.Locks;
using CustomCodeFramework.Messaging.Outbox.Options;
using CustomCodeFramework.Messaging.Outbox.Processing;
using CustomCodeFramework.Workers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Messaging.Outbox.BackgroundServices;

public sealed class OutboxBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<OutboxBackgroundServiceOptions> options,
    ILogger<OutboxBackgroundService> logger)
    : BackgroundService
{
    private readonly OutboxBackgroundServiceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Outbox background service is disabled.");
            return;
        }

        if (_options.RunImmediately)
        {
            await ExecuteOnceSafelyAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_options.PollingIntervalSeconds));

        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteOnceSafelyAsync(stoppingToken);
        }
    }

    private async Task ExecuteOnceSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteOnceAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Outbox background service execution failed.");

            await Task.Delay(
                TimeSpan.FromSeconds(_options.ErrorDelaySeconds),
                cancellationToken);
        }
    }

    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        IAsyncDisposable? lockHandle = null;

        if (_options.UseDistributedLock)
        {
            var lockProvider = scope.ServiceProvider.GetService<IWorkerLockProvider>();

            if (lockProvider is not null)
            {
                lockHandle = await lockProvider.AcquireAsync(
                    OutboxWorkerLockKeys.OutboxProcessor,
                    TimeSpan.FromSeconds(_options.LockDurationSeconds),
                    cancellationToken);

                if (lockHandle is null)
                {
                    logger.LogDebug(
                        "Outbox background service skipped because lock was not acquired.");

                    return;
                }
            }
        }

        await using (lockHandle)
        {
            var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

            OutboxProcessingResult result;

            do
            {
                result = await processor.ProcessAsync(
                    _options.BatchSize,
                    cancellationToken);

                logger.LogInformation(
                    "Outbox processed {ProcessedCount} message(s), failed {FailedCount} message(s). HasMoreMessages: {HasMoreMessages}.",
                    result.ProcessedCount,
                    result.FailedCount,
                    result.HasMoreMessages);
            }
            while (result.HasMoreMessages && !cancellationToken.IsCancellationRequested);
        }
    }
}
```

---

# InboxCleanupBackgroundService

Background service encargado de ejecutar `IInboxProcessor`.

```csharp
using CustomCodeFramework.Messaging.Outbox.Locks;
using CustomCodeFramework.Messaging.Outbox.Options;
using CustomCodeFramework.Messaging.Outbox.Processing;
using CustomCodeFramework.Workers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Messaging.Outbox.BackgroundServices;

public sealed class InboxCleanupBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<InboxCleanupOptions> options,
    ILogger<InboxCleanupBackgroundService> logger)
    : BackgroundService
{
    private readonly InboxCleanupOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Inbox cleanup background service is disabled.");
            return;
        }

        if (_options.RunImmediately)
        {
            await ExecuteOnceSafelyAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_options.PollingIntervalSeconds));

        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteOnceSafelyAsync(stoppingToken);
        }
    }

    private async Task ExecuteOnceSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteOnceAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Inbox cleanup background service execution failed.");

            await Task.Delay(
                TimeSpan.FromSeconds(_options.ErrorDelaySeconds),
                cancellationToken);
        }
    }

    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        IAsyncDisposable? lockHandle = null;

        if (_options.UseDistributedLock)
        {
            var lockProvider = scope.ServiceProvider.GetService<IWorkerLockProvider>();

            if (lockProvider is not null)
            {
                lockHandle = await lockProvider.AcquireAsync(
                    OutboxWorkerLockKeys.InboxCleanup,
                    TimeSpan.FromSeconds(_options.LockDurationSeconds),
                    cancellationToken);

                if (lockHandle is null)
                {
                    logger.LogDebug(
                        "Inbox cleanup skipped because lock was not acquired.");

                    return;
                }
            }
        }

        await using (lockHandle)
        {
            var processor = scope.ServiceProvider.GetRequiredService<IInboxProcessor>();

            var olderThan = TimeSpan.FromDays(
                _options.DeleteMessagesOlderThanDays);

            InboxProcessingResult result;

            do
            {
                result = await processor.CleanupAsync(
                    olderThan,
                    _options.BatchSize,
                    cancellationToken);

                logger.LogInformation(
                    "Inbox cleanup deleted {DeletedCount} message(s). HasMoreMessages: {HasMoreMessages}.",
                    result.DeletedCount,
                    result.HasMoreMessages);
            }
            while (result.HasMoreMessages && !cancellationToken.IsCancellationRequested);
        }
    }
}
```

---

# MessagingOutboxServiceCollectionExtensions

Registro DI.

```csharp
using CustomCodeFramework.Messaging.Outbox.BackgroundServices;
using CustomCodeFramework.Messaging.Outbox.Options;
using CustomCodeFramework.Messaging.Outbox.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Messaging.Outbox.DependencyInjection;

public static class MessagingOutboxServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeMessagingOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxBackgroundServiceOptions>(
            configuration.GetSection("Messaging:Outbox:BackgroundService"));

        services.Configure<InboxCleanupOptions>(
            configuration.GetSection("Messaging:Inbox:Cleanup"));

        return services;
    }

    public static IServiceCollection AddCustomCodeOutboxProcessor<TProcessor>(
        this IServiceCollection services)
        where TProcessor : class, IOutboxProcessor
    {
        services.AddScoped<IOutboxProcessor, TProcessor>();

        return services;
    }

    public static IServiceCollection AddCustomCodeInboxProcessor<TProcessor>(
        this IServiceCollection services)
        where TProcessor : class, IInboxProcessor
    {
        services.AddScoped<IInboxProcessor, TProcessor>();

        return services;
    }

    public static IServiceCollection AddCustomCodeOutboxBackgroundService(
        this IServiceCollection services)
    {
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }

    public static IServiceCollection AddCustomCodeInboxCleanupBackgroundService(
        this IServiceCollection services)
    {
        services.AddHostedService<InboxCleanupBackgroundService>();

        return services;
    }

    public static IServiceCollection AddCustomCodeMessagingOutboxHostedServices(
        this IServiceCollection services)
    {
        services.AddHostedService<OutboxBackgroundService>();
        services.AddHostedService<InboxCleanupBackgroundService>();

        return services;
    }
}
```

---

# appsettings.json

```json
{
  "Messaging": {
    "Outbox": {
      "BackgroundService": {
        "Enabled": true,
        "BatchSize": 50,
        "PollingIntervalSeconds": 10,
        "RunImmediately": true,
        "UseDistributedLock": true,
        "LockDurationSeconds": 60,
        "ErrorDelaySeconds": 10
      }
    },
    "Inbox": {
      "Cleanup": {
        "Enabled": true,
        "BatchSize": 100,
        "PollingIntervalSeconds": 3600,
        "DeleteMessagesOlderThanDays": 30,
        "RunImmediately": false,
        "UseDistributedLock": true,
        "LockDurationSeconds": 300,
        "ErrorDelaySeconds": 30
      }
    }
  }
}
```

---

# Registro básico

```csharp
builder.Services.AddCustomCodeMessagingOutbox(
    builder.Configuration);

builder.Services.AddCustomCodeOutboxProcessor<AppOutboxProcessor>();
builder.Services.AddCustomCodeInboxProcessor<AppInboxProcessor>();

builder.Services.AddCustomCodeMessagingOutboxHostedServices();
```

---

# Dependencias

```xml
<ItemGroup>
  <ProjectReference Include="..\CustomCodeFramework.Core\CustomCodeFramework.Core.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Messaging\CustomCodeFramework.Messaging.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Workers\CustomCodeFramework.Workers.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Redis\CustomCodeFramework.Redis.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Observability\CustomCodeFramework.Observability.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0" />
</ItemGroup>
```

---

# Integraciones previstas

Este paquete sirve como base para:

```text
CustomCodeFramework.Messaging
CustomCodeFramework.Postgres.EntityFramework
CustomCodeFramework.Redis
CustomCodeFramework.Redis.Streams
CustomCodeFramework.Workers
CustomCodeFramework.Observability
```

Ejemplos:

```text
EfOutboxProcessor
EfInboxProcessor
RedisStreamPublisher
RabbitMqPublisher
KafkaPublisher
AzureServiceBusPublisher
```

---

# Casos de uso

## Publicación confiable de eventos

```text
leer outbox_messages
publicar evento
marcar como procesado
```

## Reintentos de eventos fallidos

```text
leer mensajes failed
validar retry_count
volver a publicar
actualizar estado
```

## Limpieza de Inbox

```text
leer inbox_messages antiguos
eliminar mensajes procesados
mantener tabla liviana
```

## Evitar ejecución duplicada

```text
tomar lock distribuido
procesar mensajes
liberar lock
```

---

# Beneficios

```text
Procesamiento estándar de Outbox
Limpieza estándar de Inbox
Soporte para locks distribuidos
Reintentos controlados
Evita pérdida de eventos
Evita consumidores duplicados
Compatible con PostgreSQL
Compatible con Redis Streams
Compatible con futuros brokers
Compatible con todos los microservicios
```
