# CustomCodeFramework.Redis.Streams

## Introducción

`CustomCodeFramework.Redis.Streams` proporciona una infraestructura estándar para publicar y consumir mensajes usando Redis Streams dentro de microservicios.

Su objetivo es evitar que cada servicio implemente su propia lógica de:

- Publicación en streams.
- Consumo desde consumer groups.
- Lectura de mensajes pendientes.
- Checkpoints.
- Serialización.
- Headers.
- Reintentos.
- Background services.
- Logging.
- Observabilidad.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Redis.Streams --version 0.1.0-alpha.1
```

---

# Estructura

```text
CustomCodeFramework.Redis.Streams
├── Abstractions
│   ├── IRedisStreamPublisher.cs
│   ├── IRedisStreamConsumer.cs
│   ├── IRedisStreamMessageHandler.cs
│   └── IRedisStreamCheckpointStore.cs
│
├── Messages
│   ├── RedisStreamMessage.cs
│   ├── RedisStreamEnvelope.cs
│   └── RedisStreamHeaders.cs
│
├── Publishing
│   └── RedisStreamPublisher.cs
│
├── Consuming
│   ├── RedisStreamConsumer.cs
│   ├── RedisStreamConsumerBackgroundService.cs
│   ├── RedisStreamConsumerContext.cs
│   └── RedisStreamConsumerResult.cs
│
├── Options
│   ├── RedisStreamOptions.cs
│   ├── RedisStreamConsumerOptions.cs
│   └── RedisStreamPublisherOptions.cs
│
├── Serialization
│   └── RedisStreamMessageSerializer.cs
│
└── DependencyInjection
    └── RedisStreamsServiceCollectionExtensions.cs
```

---

# IRedisStreamPublisher

Contrato principal para publicar mensajes en Redis Streams.

```csharp
namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamPublisher
{
    Task<string> PublishAsync(
        RedisStreamMessage message,
        CancellationToken cancellationToken);
}
```

---

# IRedisStreamConsumer

Contrato principal para consumir mensajes desde Redis Streams.

```csharp
namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamConsumer
{
    Task<RedisStreamConsumerResult> ConsumeAsync(
        RedisStreamConsumerContext context,
        CancellationToken cancellationToken);
}
```

---

# IRedisStreamMessageHandler

Contrato para procesar cada mensaje recibido desde Redis Streams.

```csharp
namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamMessageHandler
{
    string MessageType { get; }

    Task HandleAsync(
        RedisStreamEnvelope envelope,
        CancellationToken cancellationToken);
}
```

---

# IRedisStreamCheckpointStore

Contrato para guardar el último mensaje procesado.

```csharp
namespace CustomCodeFramework.Redis.Streams.Abstractions;

public interface IRedisStreamCheckpointStore
{
    Task<string?> GetLastMessageIdAsync(
        string streamName,
        string consumerGroup,
        CancellationToken cancellationToken);

    Task SaveLastMessageIdAsync(
        string streamName,
        string consumerGroup,
        string messageId,
        CancellationToken cancellationToken);
}
```

---

# RedisStreamMessage

Representa un mensaje que se publicará en Redis Stream.

```csharp
namespace CustomCodeFramework.Redis.Streams.Messages;

public sealed class RedisStreamMessage
{
    public string StreamName { get; init; } = default!;

    public string MessageType { get; init; } = default!;

    public object Payload { get; init; } = default!;

    public Dictionary<string, string> Headers { get; init; } = [];
}
```

---

# RedisStreamEnvelope

Representa un mensaje recibido desde Redis Stream.

```csharp
namespace CustomCodeFramework.Redis.Streams.Messages;

public sealed class RedisStreamEnvelope
{
    public string StreamName { get; init; } = default!;

    public string MessageId { get; init; } = default!;

    public string MessageType { get; init; } = default!;

    public string PayloadJson { get; init; } = default!;

    public Dictionary<string, string> Headers { get; init; } = [];

    public DateTime ReceivedAtUtc { get; init; } = DateTime.UtcNow;
}
```

---

# RedisStreamHeaders

Headers estándar para mensajes.

```csharp
namespace CustomCodeFramework.Redis.Streams.Messages;

public static class RedisStreamHeaders
{
    public const string MessageId = "message_id";

    public const string MessageType = "message_type";

    public const string CorrelationId = "correlation_id";

    public const string CausationId = "causation_id";

    public const string SourceService = "source_service";

    public const string CreatedAtUtc = "created_at_utc";
}
```

---

# RedisStreamOptions

Configuración general de Redis Streams.

```csharp
namespace CustomCodeFramework.Redis.Streams.Options;

public sealed class RedisStreamOptions
{
    public bool Enabled { get; set; } = true;

    public string DefaultStreamName { get; set; } = "events";

    public string SourceService { get; set; } = "unknown-service";
}
```

---

# RedisStreamConsumerOptions

Configuración del consumer.

```csharp
namespace CustomCodeFramework.Redis.Streams.Options;

public sealed class RedisStreamConsumerOptions
{
    public bool Enabled { get; set; } = true;

    public string StreamName { get; set; } = "events";

    public string ConsumerGroup { get; set; } = "default-group";

    public string ConsumerName { get; set; } = Environment.MachineName;

    public int BatchSize { get; set; } = 10;

    public int PollingIntervalSeconds { get; set; } = 5;

    public bool CreateConsumerGroupIfNotExists { get; set; } = true;

    public bool ReadPendingMessages { get; set; } = true;

    public int ErrorDelaySeconds { get; set; } = 10;
}
```

---

# RedisStreamPublisherOptions

Configuración del publisher.

```csharp
namespace CustomCodeFramework.Redis.Streams.Options;

public sealed class RedisStreamPublisherOptions
{
    public int MaxLength { get; set; } = 100000;

    public bool UseApproximateMaxLength { get; set; } = true;
}
```

---

# RedisStreamMessageSerializer

Serializador base para payloads.

```csharp
using System.Text.Json;

namespace CustomCodeFramework.Redis.Streams.Serialization;

public sealed class RedisStreamMessageSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}
```

---

# RedisStreamPublisher

Publicador de mensajes en Redis Streams.

```csharp
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using CustomCodeFramework.Redis.Streams.Options;
using CustomCodeFramework.Redis.Streams.Serialization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.Streams.Publishing;

public sealed class RedisStreamPublisher(
    IConnectionMultiplexer connectionMultiplexer,
    RedisStreamMessageSerializer serializer,
    IOptions<RedisStreamOptions> streamOptions,
    IOptions<RedisStreamPublisherOptions> publisherOptions)
    : IRedisStreamPublisher
{
    private readonly RedisStreamOptions _streamOptions = streamOptions.Value;
    private readonly RedisStreamPublisherOptions _publisherOptions = publisherOptions.Value;

    public async Task<string> PublishAsync(
        RedisStreamMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.MessageType);

        var database = connectionMultiplexer.GetDatabase();

        var streamName = string.IsNullOrWhiteSpace(message.StreamName)
            ? _streamOptions.DefaultStreamName
            : message.StreamName;

        var payloadJson = serializer.Serialize(message.Payload);

        var headers = new Dictionary<string, string>(message.Headers)
        {
            [RedisStreamHeaders.MessageId] = Guid.NewGuid().ToString(),
            [RedisStreamHeaders.MessageType] = message.MessageType,
            [RedisStreamHeaders.SourceService] = _streamOptions.SourceService,
            [RedisStreamHeaders.CreatedAtUtc] = DateTime.UtcNow.ToString("O")
        };

        var entries = new List<NameValueEntry>
        {
            new("message_type", message.MessageType),
            new("payload_json", payloadJson)
        };

        foreach (var header in headers)
        {
            entries.Add(new NameValueEntry($"header:{header.Key}", header.Value));
        }

        var messageId = await database.StreamAddAsync(
            streamName,
            entries.ToArray(),
            maxLength: _publisherOptions.MaxLength,
            useApproximateMaxLength: _publisherOptions.UseApproximateMaxLength);

        return messageId.ToString();
    }
}
```

---

# RedisStreamConsumerContext

Contexto de consumo.

```csharp
namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed class RedisStreamConsumerContext
{
    public string StreamName { get; init; } = default!;

    public string ConsumerGroup { get; init; } = default!;

    public string ConsumerName { get; init; } = default!;

    public int BatchSize { get; init; } = 10;

    public bool ReadPendingMessages { get; init; } = true;
}
```

---

# RedisStreamConsumerResult

Resultado de una ejecución de consumo.

```csharp
namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed record RedisStreamConsumerResult(
    int ProcessedCount,
    int FailedCount,
    bool HasMessages)
{
    public static RedisStreamConsumerResult Empty { get; } = new(
        ProcessedCount: 0,
        FailedCount: 0,
        HasMessages: false);
}
```

---

# RedisStreamConsumer

Consumidor de Redis Streams.

```csharp
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Consuming;
using CustomCodeFramework.Redis.Streams.Messages;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed class RedisStreamConsumer(
    IConnectionMultiplexer connectionMultiplexer,
    IEnumerable<IRedisStreamMessageHandler> handlers)
    : IRedisStreamConsumer
{
    public async Task<RedisStreamConsumerResult> ConsumeAsync(
        RedisStreamConsumerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var database = connectionMultiplexer.GetDatabase();

        await EnsureConsumerGroupAsync(
            database,
            context.StreamName,
            context.ConsumerGroup);

        var entries = await database.StreamReadGroupAsync(
            context.StreamName,
            context.ConsumerGroup,
            context.ConsumerName,
            ">",
            context.BatchSize);

        if (entries.Length == 0)
        {
            return RedisStreamConsumerResult.Empty;
        }

        var processed = 0;
        var failed = 0;

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var envelope = MapEnvelope(
                context.StreamName,
                entry);

            var handler = handlers.FirstOrDefault(x =>
                x.MessageType.Equals(
                    envelope.MessageType,
                    StringComparison.OrdinalIgnoreCase));

            if (handler is null)
            {
                failed++;
                continue;
            }

            try
            {
                await handler.HandleAsync(
                    envelope,
                    cancellationToken);

                await database.StreamAcknowledgeAsync(
                    context.StreamName,
                    context.ConsumerGroup,
                    entry.Id);

                processed++;
            }
            catch
            {
                failed++;
            }
        }

        return new RedisStreamConsumerResult(
            processed,
            failed,
            HasMessages: entries.Length > 0);
    }

    private static async Task EnsureConsumerGroupAsync(
        IDatabase database,
        string streamName,
        string consumerGroup)
    {
        try
        {
            await database.StreamCreateConsumerGroupAsync(
                streamName,
                consumerGroup,
                StreamPosition.NewMessages,
                createStream: true);
        }
        catch (RedisServerException exception)
            when (exception.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    private static RedisStreamEnvelope MapEnvelope(
        string streamName,
        StreamEntry entry)
    {
        var values = entry.Values.ToDictionary(
            x => x.Name.ToString(),
            x => x.Value.ToString());

        var headers = values
            .Where(x => x.Key.StartsWith("header:", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                x => x.Key["header:".Length..],
                x => x.Value);

        return new RedisStreamEnvelope
        {
            StreamName = streamName,
            MessageId = entry.Id.ToString(),
            MessageType = values.GetValueOrDefault("message_type") ?? string.Empty,
            PayloadJson = values.GetValueOrDefault("payload_json") ?? string.Empty,
            Headers = headers,
            ReceivedAtUtc = DateTime.UtcNow
        };
    }
}
```

---

# RedisStreamConsumerBackgroundService

Background service encargado de consumir Redis Streams.

```csharp
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Consuming;
using CustomCodeFramework.Redis.Streams.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed class RedisStreamConsumerBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<RedisStreamConsumerOptions> options,
    ILogger<RedisStreamConsumerBackgroundService> logger)
    : BackgroundService
{
    private readonly RedisStreamConsumerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Redis Stream consumer background service is disabled.");
            return;
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
            using var scope = serviceProvider.CreateScope();

            var consumer = scope.ServiceProvider.GetRequiredService<IRedisStreamConsumer>();

            var context = new RedisStreamConsumerContext
            {
                StreamName = _options.StreamName,
                ConsumerGroup = _options.ConsumerGroup,
                ConsumerName = _options.ConsumerName,
                BatchSize = _options.BatchSize,
                ReadPendingMessages = _options.ReadPendingMessages
            };

            var result = await consumer.ConsumeAsync(
                context,
                cancellationToken);

            if (result.HasMessages)
            {
                logger.LogInformation(
                    "Redis Stream consumer processed {ProcessedCount} message(s), failed {FailedCount} message(s).",
                    result.ProcessedCount,
                    result.FailedCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Redis Stream consumer execution failed.");

            await Task.Delay(
                TimeSpan.FromSeconds(_options.ErrorDelaySeconds),
                cancellationToken);
        }
    }
}
```

---

# RedisStreamsServiceCollectionExtensions

Registro DI.

```csharp
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Consuming;
using CustomCodeFramework.Redis.Streams.Options;
using CustomCodeFramework.Redis.Streams.Publishing;
using CustomCodeFramework.Redis.Streams.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Redis.Streams.DependencyInjection;

public static class RedisStreamsServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeRedisStreams(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisStreamOptions>(
            configuration.GetSection("Redis:Streams"));

        services.Configure<RedisStreamConsumerOptions>(
            configuration.GetSection("Redis:Streams:Consumer"));

        services.Configure<RedisStreamPublisherOptions>(
            configuration.GetSection("Redis:Streams:Publisher"));

        services.AddSingleton<RedisStreamMessageSerializer>();
        services.AddScoped<IRedisStreamPublisher, RedisStreamPublisher>();
        services.AddScoped<IRedisStreamConsumer, RedisStreamConsumer>();

        return services;
    }

    public static IServiceCollection AddCustomCodeRedisStreamHandler<THandler>(
        this IServiceCollection services)
        where THandler : class, IRedisStreamMessageHandler
    {
        services.AddScoped<IRedisStreamMessageHandler, THandler>();

        return services;
    }

    public static IServiceCollection AddCustomCodeRedisStreamConsumerBackgroundService(
        this IServiceCollection services)
    {
        services.AddHostedService<RedisStreamConsumerBackgroundService>();

        return services;
    }
}
```

---

# appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Streams": {
      "Enabled": true,
      "DefaultStreamName": "dhole-events",
      "SourceService": "Dhole.Crm",
      "Consumer": {
        "Enabled": true,
        "StreamName": "dhole-events",
        "ConsumerGroup": "crm-service",
        "ConsumerName": "crm-worker-1",
        "BatchSize": 10,
        "PollingIntervalSeconds": 5,
        "CreateConsumerGroupIfNotExists": true,
        "ReadPendingMessages": true,
        "ErrorDelaySeconds": 10
      },
      "Publisher": {
        "MaxLength": 100000,
        "UseApproximateMaxLength": true
      }
    }
  }
}
```

---

# Registro básico

```csharp
builder.Services.AddCustomCodeRedis(
    builder.Configuration);

builder.Services.AddCustomCodeRedisStreams(
    builder.Configuration);

builder.Services.AddCustomCodeRedisStreamHandler<ClientCreatedStreamHandler>();

builder.Services.AddCustomCodeRedisStreamConsumerBackgroundService();
```

---

# Dependencias

```xml
<ItemGroup>
  <ProjectReference Include="..\CustomCodeFramework.Core\CustomCodeFramework.Core.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Redis\CustomCodeFramework.Redis.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Messaging\CustomCodeFramework.Messaging.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Workers\CustomCodeFramework.Workers.csproj" />
  <ProjectReference Include="..\CustomCodeFramework.Observability\CustomCodeFramework.Observability.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="StackExchange.Redis" Version="2.9.32" />
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
CustomCodeFramework.Messaging.Outbox
CustomCodeFramework.Redis
CustomCodeFramework.Workers
CustomCodeFramework.Observability
```

Ejemplos:

```text
OutboxBackgroundService publica a Redis Streams
RedisStreamsConsumerBackgroundService consume eventos
Inbox registra mensajes procesados
Observability registra logs y métricas
Workers estandariza ejecución en segundo plano
```

---

# Casos de uso

## Publicación de eventos livianos

```text
crear mensaje
serializar payload
publicar en stream
retornar message id
```

## Consumo de eventos

```text
leer stream
buscar handler por message_type
procesar mensaje
acknowledge
```

## Integración entre microservicios

```text
CRM publica evento
Pricing consume evento
Quotes consume evento
Notifications consume evento
```

## Procesamiento con consumer groups

```text
consumer group por servicio
consumer name por instancia
ack por mensaje procesado
```

---

# Beneficios

```text
Comunicación simple entre microservicios
Soporte para Redis Streams
Consumer groups estándar
Publicación centralizada
Consumo centralizado
Handlers por tipo de mensaje
Headers estándar
Compatible con Outbox
Compatible con Inbox
Compatible con Workers
Compatible con Observability
```
