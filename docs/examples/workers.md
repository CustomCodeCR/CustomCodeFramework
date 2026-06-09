# CustomCodeFramework.Workers

## Introducción

`CustomCodeFramework.Workers` proporciona una infraestructura estándar para la ejecución de procesos en segundo plano (Background Services) dentro de cualquier microservicio.

Su objetivo es evitar que cada servicio implemente su propia lógica de:

- Timers
- Reintentos
- Locks distribuidos
- Manejo de errores
- Ejecución periódica
- Logging
- Observabilidad

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Workers --version 0.1.0-alpha.1
```

---

# Estructura

```text
CustomCodeFramework.Workers
├── Abstractions
│   ├── IBackgroundWorker.cs
│   ├── IWorkerExecutionContext.cs
│   └── IWorkerLockProvider.cs
│
├── HostedServices
│   ├── BackgroundWorkerService.cs
│   ├── PeriodicBackgroundService.cs
│   └── ResilientBackgroundService.cs
│
├── Scheduling
│   ├── WorkerScheduleOptions.cs
│   └── WorkerTimer.cs
│
├── Options
│   └── WorkerOptions.cs
│
└── DependencyInjection
    └── WorkerServiceCollectionExtensions.cs
```

---

# IBackgroundWorker

Contrato principal para cualquier worker.

```csharp
namespace CustomCodeFramework.Workers.Abstractions;

public interface IBackgroundWorker
{
    Task ExecuteAsync(
        IWorkerExecutionContext context,
        CancellationToken cancellationToken);
}
```

---

# IWorkerExecutionContext

Información de ejecución.

```csharp
namespace CustomCodeFramework.Workers.Abstractions;

public interface IWorkerExecutionContext
{
    Guid ExecutionId { get; }

    DateTime StartedAtUtc { get; }

    string WorkerName { get; }

    IServiceProvider Services { get; }
}
```

---

# IWorkerLockProvider

Permite evitar ejecución simultánea.

Ideal para Redis Distributed Lock.

```csharp
namespace CustomCodeFramework.Workers.Abstractions;

public interface IWorkerLockProvider
{
    Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken);
}
```

---

# WorkerOptions

Configuración general.

```csharp
namespace CustomCodeFramework.Workers.Options;

public sealed class WorkerOptions
{
    public bool Enabled { get; set; } = true;

    public int MaxRetryCount { get; set; } = 3;

    public int RetryDelaySeconds { get; set; } = 5;

    public bool UseDistributedLock { get; set; }

    public int LockDurationSeconds { get; set; } = 60;
}
```

---

# WorkerScheduleOptions

Configuración de frecuencia.

```csharp
namespace CustomCodeFramework.Workers.Scheduling;

public sealed class WorkerScheduleOptions
{
    public TimeSpan Interval { get; set; }

    public bool RunImmediately { get; set; } = true;
}
```

---

# WorkerTimer

Timer reutilizable.

```csharp
namespace CustomCodeFramework.Workers.Scheduling;

public sealed class WorkerTimer
{
    private readonly PeriodicTimer _timer;

    public WorkerTimer(TimeSpan interval)
    {
        _timer = new PeriodicTimer(interval);
    }

    public ValueTask<bool> WaitForNextTickAsync(
        CancellationToken cancellationToken)
    {
        return _timer.WaitForNextTickAsync(cancellationToken);
    }
}
```

---

# BackgroundWorkerService

Clase base mínima.

```csharp
using Microsoft.Extensions.Hosting;

namespace CustomCodeFramework.Workers.HostedServices;

public abstract class BackgroundWorkerService : BackgroundService
{
}
```

---

# PeriodicBackgroundService

Clase base para workers periódicos.

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomCodeFramework.Workers.HostedServices;

public abstract class PeriodicBackgroundService(
    ILogger logger,
    TimeSpan interval)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(interval);

        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Worker execution failed.");
            }
        }
    }

    protected abstract Task ProcessAsync(
        CancellationToken cancellationToken);
}
```

---

# ResilientBackgroundService

Worker con reintentos automáticos.

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomCodeFramework.Workers.HostedServices;

public abstract class ResilientBackgroundService(
    ILogger logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Worker execution failed.");

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }
    }

    protected abstract Task ProcessAsync(
        CancellationToken cancellationToken);
}
```

---

# WorkerServiceCollectionExtensions

Registro DI.

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Workers.DependencyInjection;

public static class WorkerServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeWorkers(
        this IServiceCollection services)
    {
        return services;
    }
}
```

---

# appsettings.json

```json
{
  "Workers": {
    "Enabled": true,
    "MaxRetryCount": 3,
    "RetryDelaySeconds": 5,
    "UseDistributedLock": true,
    "LockDurationSeconds": 60
  }
}
```

---

# Registro básico

```csharp
builder.Services.AddCustomCodeWorkers();
```

---

# Dependencias

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0" />
</ItemGroup>
```

---

# Integraciones previstas

Este paquete sirve como base para:

```text
CustomCodeFramework.Messaging.Outbox
CustomCodeFramework.Redis.Streams
CustomCodeFramework.Notifications
CustomCodeFramework.Reports
```

Ejemplos:

```text
OutboxBackgroundService
InboxCleanupBackgroundService
RedisStreamsConsumerBackgroundService
NotificationDispatcherBackgroundService
ReportGenerationBackgroundService
```

---

# Casos de uso

## Procesamiento de Outbox

```text
leer mensajes pendientes
publicar evento
marcar como procesado
```

## Consumo de Redis Streams

```text
leer stream
procesar evento
guardar checkpoint
```

## Notificaciones

```text
leer cola
enviar email
enviar sms
enviar whatsapp
```

## Reportes

```text
generar reporte
guardar archivo
notificar usuario
```

---

# Beneficios

```text
Estandarización de workers
Manejo centralizado de errores
Soporte para locks distribuidos
Reintentos automáticos
Observabilidad uniforme
Menos código repetido
Compatible con todos los microservicios
```
