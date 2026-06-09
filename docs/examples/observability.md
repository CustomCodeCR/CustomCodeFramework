# CustomCodeFramework.Observability

## Introducción

`CustomCodeFramework.Observability` es el paquete encargado de centralizar observabilidad dentro de CustomCodeFramework.

Observabilidad significa poder responder preguntas como:

```text
¿Qué pasó?
¿Cuándo pasó?
Dónde pasó?
Quién lo ejecutó?
Cuánto duró?
Falló o fue exitoso?
Qué request lo originó?
Qué servicio lo procesó?
Qué dependencia falló?
```

Este paquete busca estandarizar:

- Logging.
- Correlation ID.
- Distributed tracing.
- Metrics.
- Health checks.
- OpenTelemetry.
- Activity sources.
- Log enrichment.
- Response de health checks.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Observability --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Observability": {
    "ServiceName": "Clients.Api",
    "ServiceVersion": "0.1.0",
    "Environment": "Development",
    "OtlpEndpoint": "http://localhost:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableConsoleExporter": true
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Observability.DependencyInjection;

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "Clients.Api",
    serviceVersion: "0.1.0");
```

---

# Estructura

```text
CustomCodeFramework.Observability
├── Logging
│   ├── SerilogExtensions.cs
│   └── LogEnricher.cs
│
├── Tracing
│   ├── ActivitySources.cs
│   └── OpenTelemetryTracingExtensions.cs
│
├── Metrics
│   ├── MeterProvider.cs
│   └── OpenTelemetryMetricsExtensions.cs
│
├── HealthChecks
│   ├── HealthCheckExtensions.cs
│   └── HealthCheckResponseWriter.cs
│
├── Correlation
│   ├── CorrelationContext.cs
│   └── CorrelationIdAccessor.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# ¿Qué problema resuelve?

Sin observabilidad estandarizada, cada servicio termina logueando diferente:

```csharp
Console.WriteLine("error");
```

O:

```csharp
logger.LogInformation("Creating client");
```

Sin contexto suficiente:

```text
sin correlation id
sin user id
sin duración
sin trace id
sin service name
sin environment
sin métricas
sin health check estándar
```

Con `CustomCodeFramework.Observability`:

```text
Request
    ↓
Correlation ID
    ↓
Logs estructurados
    ↓
Traces
    ↓
Metrics
    ↓
Health Checks
```

---

# Conceptos principales

## Logging

Responde:

```text
¿Qué ocurrió?
```

Ejemplo:

```text
Client created successfully
```

---

## Tracing

Responde:

```text
Por dónde pasó una operación?
Cuánto duró cada paso?
Qué dependencia tardó?
```

Ejemplo:

```text
HTTP POST /clients
    ↓
CreateClientCommandHandler
    ↓
PostgreSQL insert
    ↓
Outbox save
```

---

## Metrics

Responde:

```text
Cuántas veces pasó?
Qué tan rápido?
Qué tanto falla?
```

Ejemplos:

```text
requests_total
request_duration_ms
clients_created_total
outbox_messages_processed_total
```

---

## Health Checks

Responde:

```text
El servicio está vivo?
Puede conectarse a PostgreSQL?
Puede conectarse a Redis?
Puede conectarse a Mongo?
```

---

## Correlation ID

Permite rastrear la misma operación entre servicios.

Ejemplo:

```text
X-Correlation-Id: corr-123
```

Flujo:

```text
Gateway
    ↓ corr-123
Clients.Api
    ↓ corr-123
Messaging
    ↓ corr-123
Notifications.Api
```

---

# CorrelationContext

## ¿Para qué sirve?

Guarda el correlation id actual de la petición.

---

## Uso

```csharp
using CustomCodeFramework.Observability.Correlation;

public sealed class ClientAuditService(
    CorrelationContext correlationContext)
{
    public string GetCorrelationId()
    {
        return correlationContext.CorrelationId;
    }
}
```

---

# CorrelationIdAccessor

## ¿Para qué sirve?

Permite acceder al correlation id desde servicios internos.

---

## Uso

```csharp
using CustomCodeFramework.Observability.Correlation;

public sealed class ReportService(
    CorrelationIdAccessor correlationIdAccessor)
{
    public void Generate()
    {
        var correlationId = correlationIdAccessor.CorrelationId;

        // Usar correlationId en logs o eventos.
    }
}
```

---

# Logging con Serilog

## Configuración recomendada

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

# SerilogExtensions

## Registro conceptual

```csharp
using CustomCodeFramework.Observability.Logging;

builder.Host.UseCustomCodeSerilog(
    builder.Configuration,
    serviceName: "Clients.Api");
```

---

# Logs estructurados

## Incorrecto

```csharp
logger.LogInformation("Client created " + clientId);
```

---

## Correcto

```csharp
logger.LogInformation(
    "Client {ClientId} created successfully",
    clientId);
```

Ventajas:

```text
filtrable por ClientId
indexable
compatible con Loki/Seq/Elastic
más fácil de buscar
```

---

# LogEnricher

## ¿Para qué sirve?

Agrega información común a todos los logs:

```text
ServiceName
ServiceVersion
Environment
CorrelationId
TraceId
UserId
```

---

## Ejemplo de log enriquecido

```json
{
  "timestamp": "2026-06-09T15:30:00Z",
  "level": "Information",
  "message": "Client 123 created successfully",
  "serviceName": "Clients.Api",
  "serviceVersion": "0.1.0",
  "environment": "Development",
  "correlationId": "corr-123",
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "userId": "user-001"
}
```

---

# Logging en CommandHandler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using Microsoft.Extensions.Logging;

public sealed class CreateClientCommandHandler(
    ILogger<CreateClientCommandHandler> logger)
    : ICommandHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Creating client with email {Email}",
            command.Email);

        var clientId = Guid.NewGuid();

        logger.LogInformation(
            "Client {ClientId} created successfully",
            clientId);

        return clientId;
    }
}
```

---

# Logging de errores

```csharp
try
{
    await service.ExecuteAsync(cancellationToken);
}
catch (Exception exception)
{
    logger.LogError(
        exception,
        "Error while processing report {ReportId}",
        reportId);

    throw;
}
```

---

# Logging de negocio

Usar logs de negocio para eventos importantes:

```csharp
logger.LogInformation(
    "Sales order {SalesOrderId} approved by user {UserId}",
    salesOrderId,
    currentUser.UserId);
```

No loguear datos sensibles:

```text
passwords
tokens
api keys
secret keys
credit card numbers
personal documents
```

---

# ActivitySources

## ¿Para qué sirve?

Define fuentes de trazas para OpenTelemetry.

Ejemplo conceptual:

```csharp
ActivitySources.Default
ActivitySources.Application
ActivitySources.Infrastructure
```

---

# Crear una traza manual

```csharp
using System.Diagnostics;
using CustomCodeFramework.Observability.Tracing;

public sealed class ClientImporter
{
    public async Task ImportAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Application.StartActivity(
            "clients.import");

        activity?.SetTag("import.source", "csv");
        activity?.SetTag("import.type", "clients");

        // Ejecutar importación.

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
```

---

# Traza con error

```csharp
using System.Diagnostics;
using CustomCodeFramework.Observability.Tracing;

public sealed class ReportGenerator
{
    public async Task GenerateAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Application.StartActivity(
            "reports.generate");

        try
        {
            // Generar reporte.

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            activity?.RecordException(exception);

            throw;
        }
    }
}
```

---

# OpenTelemetry Tracing

## ¿Para qué sirve?

Permite exportar trazas a herramientas como:

```text
Jaeger
Grafana Tempo
Zipkin
Azure Monitor
Datadog
New Relic
OpenTelemetry Collector
```

---

## Registro conceptual

```csharp
using CustomCodeFramework.Observability.Tracing;

builder.Services.AddCustomCodeOpenTelemetryTracing(
    builder.Configuration,
    serviceName: "Clients.Api",
    serviceVersion: "0.1.0");
```

---

# Configuración OTLP

```json
{
  "Observability": {
    "OtlpEndpoint": "http://localhost:4317",
    "EnableTracing": true
  }
}
```

---

# Trace esperado

```text
POST /api/v1/clients
    ↓
CreateClientEndpoint
    ↓
CreateClientCommandHandler
    ↓
PostgreSQL SaveChanges
    ↓
Outbox Insert
```

---

# Tags recomendados

## HTTP

```text
http.method
http.route
http.status_code
```

---

## Usuario

```text
user.id
tenant.id
```

---

## Negocio

```text
client.id
sales_order.id
invoice.id
report.id
```

---

## Infraestructura

```text
db.system
db.name
messaging.system
cache.system
```

---

# Metrics

## ¿Para qué sirven?

Las métricas permiten medir comportamiento del sistema.

Ejemplos:

```text
requests count
request duration
commands handled
queries handled
cache hits
cache misses
outbox processed
notifications sent
reports generated
```

---

# MeterProvider

Define meters del framework.

Ejemplo conceptual:

```csharp
using CustomCodeFramework.Observability.Metrics;

meterProvider.IncrementCounter(
    "clients.created.total");
```

---

# Crear métrica manual

```csharp
using System.Diagnostics.Metrics;

public sealed class ClientMetrics
{
    private static readonly Meter Meter = new(
        "Clients.Api",
        "0.1.0");

    private static readonly Counter<long> ClientsCreatedCounter =
        Meter.CreateCounter<long>("clients_created_total");

    public void ClientCreated()
    {
        ClientsCreatedCounter.Add(1);
    }
}
```

---

# Métrica con tags

```csharp
private static readonly Counter<long> NotificationsSentCounter =
    Meter.CreateCounter<long>("notifications_sent_total");

public void NotificationSent(string channel)
{
    NotificationsSentCounter.Add(
        1,
        new KeyValuePair<string, object?>("channel", channel));
}
```

---

# Histogram para duración

```csharp
private static readonly Histogram<double> ReportDuration =
    Meter.CreateHistogram<double>("reports_duration_ms");

public async Task MeasureAsync(Func<Task> action)
{
    var startedAt = Stopwatch.GetTimestamp();

    await action();

    var elapsed = Stopwatch.GetElapsedTime(startedAt);

    ReportDuration.Record(elapsed.TotalMilliseconds);
}
```

---

# OpenTelemetry Metrics

## Registro conceptual

```csharp
using CustomCodeFramework.Observability.Metrics;

builder.Services.AddCustomCodeOpenTelemetryMetrics(
    builder.Configuration,
    serviceName: "Clients.Api",
    serviceVersion: "0.1.0");
```

---

# Métricas recomendadas por servicio

## API

```text
http_requests_total
http_request_duration_ms
http_requests_failed_total
```

---

## CQRS

```text
commands_handled_total
commands_failed_total
command_duration_ms
queries_handled_total
query_duration_ms
```

---

## Redis

```text
cache_hits_total
cache_misses_total
distributed_locks_acquired_total
distributed_locks_failed_total
```

---

## Messaging

```text
outbox_messages_processed_total
outbox_messages_failed_total
inbox_messages_processed_total
events_published_total
events_consumed_total
```

---

## Reports

```text
reports_generated_total
reports_failed_total
report_generation_duration_ms
```

---

## Notifications

```text
notifications_sent_total
notifications_failed_total
notification_delivery_duration_ms
```

---

# Health Checks

## ¿Para qué sirven?

Sirven para saber si un servicio está disponible.

Tipos:

```text
liveness
readiness
dependency health
```

---

# Liveness

Pregunta:

```text
El proceso está vivo?
```

Endpoint:

```text
/health/live
```

---

# Readiness

Pregunta:

```text
El servicio está listo para recibir tráfico?
```

Endpoint:

```text
/health/ready
```

---

# Dependencies

Pregunta:

```text
Postgres responde?
Redis responde?
Mongo responde?
Storage responde?
```

Endpoint:

```text
/health
```

---

# HealthCheckExtensions

## Registro

```csharp
using CustomCodeFramework.Observability.HealthChecks;

builder.Services.AddCustomCodeHealthChecks();
```

---

## Uso

```csharp
using CustomCodeFramework.Observability.HealthChecks;

app.UseCustomCodeHealthChecks("/health");
```

---

# HealthCheckResponseWriter

## ¿Para qué sirve?

Devuelve health checks en formato JSON estándar.

Ejemplo:

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.023",
  "entries": {
    "postgres": {
      "status": "Healthy",
      "duration": "00:00:00.010"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.004"
    }
  }
}
```

---

# Health checks en Program.cs

```csharp
using CustomCodeFramework.Observability.HealthChecks;

builder.Services.AddCustomCodeHealthChecks();

var app = builder.Build();

app.UseCustomCodeHealthChecks("/health");
```

---

# Health checks con dependencias

```csharp
builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgres")
    .AddRedis(
        redisConnectionString,
        name: "redis")
    .AddMongoDb(
        mongoConnectionString,
        name: "mongo");
```

---

# Ejemplo completo Program.cs

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;
using CustomCodeFramework.Observability.HealthChecks;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "Clients.Api",
    serviceVersion: "0.1.0");

builder.Services.AddCustomCodeRedis(builder.Configuration);

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.UseCustomCodeHealthChecks("/health");

app.MapControllers();

app.Run();
```

---

# Ejemplo con Correlation ID

## Request

```bash
curl https://localhost:5001/api/v1/clients \
  -H "X-Correlation-Id: corr-123"
```

---

## Endpoint

```csharp
using CustomCodeFramework.Observability.Correlation;

app.MapGet("/api/v1/correlation", (
    CorrelationContext correlationContext) =>
{
    return Results.Ok(new
    {
        correlationContext.CorrelationId
    });
});
```

---

## Respuesta

```json
{
  "correlationId": "corr-123"
}
```

---

# Ejemplo de logs con correlation id

```csharp
using Microsoft.Extensions.Logging;

public sealed class CreateClientCommandHandler(
    ILogger<CreateClientCommandHandler> logger)
{
    public Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var clientId = Guid.NewGuid();

        logger.LogInformation(
            "Client {ClientId} created",
            clientId);

        return Task.FromResult(clientId);
    }
}
```

Log esperado:

```json
{
  "message": "Client dceff61f created",
  "correlationId": "corr-123",
  "serviceName": "Clients.Api"
}
```

---

# Ejemplo: Instrumentar una operación de negocio

```csharp
using System.Diagnostics;
using CustomCodeFramework.Observability.Tracing;
using Microsoft.Extensions.Logging;

public sealed class InvoiceIssuer(
    ILogger<InvoiceIssuer> logger)
{
    public async Task IssueAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Application.StartActivity(
            "invoices.issue");

        activity?.SetTag("invoice.id", invoiceId);

        logger.LogInformation(
            "Issuing invoice {InvoiceId}",
            invoiceId);

        try
        {
            // Emitir factura.

            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation(
                "Invoice {InvoiceId} issued successfully",
                invoiceId);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            activity?.RecordException(exception);

            logger.LogError(
                exception,
                "Error issuing invoice {InvoiceId}",
                invoiceId);

            throw;
        }
    }
}
```

---

# Ejemplo: Medir duración de reportes

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

public sealed class ReportGenerator(
    ILogger<ReportGenerator> logger)
{
    private static readonly Meter Meter = new(
        "Reports.Api",
        "0.1.0");

    private static readonly Counter<long> ReportsGenerated =
        Meter.CreateCounter<long>("reports_generated_total");

    private static readonly Histogram<double> ReportDuration =
        Meter.CreateHistogram<double>("report_generation_duration_ms");

    public async Task GenerateAsync(
        string reportKey,
        CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            logger.LogInformation(
                "Generating report {ReportKey}",
                reportKey);

            // Generar reporte.

            ReportsGenerated.Add(
                1,
                new KeyValuePair<string, object?>("report.key", reportKey));

            logger.LogInformation(
                "Report {ReportKey} generated",
                reportKey);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startedAt);

            ReportDuration.Record(
                elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("report.key", reportKey));
        }
    }
}
```

---

# Integración con Api

`CustomCodeFramework.Api` debe manejar:

```text
exception handler
correlation middleware
request logging middleware
```

`CustomCodeFramework.Observability` provee:

```text
correlation context
logging enrichment
tracing
metrics
health check response
```

---

# Integración con CQRS

Los behaviors de CQRS pueden generar logs y métricas.

Flujo:

```text
CommandDispatcher
    ↓
LoggingBehavior
    ↓
Metrics
    ↓
Tracing
    ↓
Handler
```

Ejemplo conceptual:

```text
CreateClientCommand handled in 45ms
```

---

# Integración con Redis

Redis puede aportar health checks y métricas:

```text
redis_health_status
cache_hits_total
cache_misses_total
distributed_locks_acquired_total
```

---

# Integración con Messaging

Messaging puede propagar correlation id.

```text
HTTP Request corr-123
    ↓
IntegrationEvent metadata correlationId=corr-123
    ↓
Consumer logs corr-123
```

---

# Integración con Reports

Reports debe emitir métricas:

```text
reports_generated_total
reports_failed_total
report_generation_duration_ms
```

---

# Integración con Notifications

Notifications debe emitir:

```text
notifications_sent_total
notifications_failed_total
notification_channel_duration_ms
```

---

# Docker Compose: Observability stack básico

## OpenTelemetry Collector + Jaeger

```yaml
services:
  jaeger:
    image: jaegertracing/all-in-one:1.57
    container_name: customcode-jaeger
    ports:
      - "16686:16686"
      - "4317:4317"
      - "4318:4318"
    environment:
      COLLECTOR_OTLP_ENABLED: "true"
```

Config appsettings:

```json
{
  "Observability": {
    "OtlpEndpoint": "http://localhost:4317",
    "EnableTracing": true,
    "EnableMetrics": true
  }
}
```

Abrir Jaeger:

```text
http://localhost:16686
```

---

# Docker Compose: Seq para logs

```yaml
services:
  seq:
    image: datalust/seq:latest
    container_name: customcode-seq
    ports:
      - "5341:80"
    environment:
      ACCEPT_EULA: "Y"
```

Abrir Seq:

```text
http://localhost:5341
```

---

# Organización recomendada

```text
Infrastructure
└── Observability
    ├── Logging
    │   └── SerilogConfiguration.cs
    │
    ├── Metrics
    │   ├── ClientMetrics.cs
    │   ├── ReportMetrics.cs
    │   └── NotificationMetrics.cs
    │
    └── Tracing
        ├── ClientActivityNames.cs
        └── ReportActivityNames.cs
```

---

# Nombres recomendados de activities

```text
clients.create
clients.update
clients.delete
clients.search

sales_orders.create
sales_orders.approve
sales_orders.cancel

invoices.issue
invoices.cancel

reports.generate
reports.export

notifications.send
storage.upload
storage.download
```

---

# Nombres recomendados de métricas

```text
clients_created_total
clients_updated_total
clients_deleted_total

sales_orders_created_total
sales_orders_approved_total

invoices_issued_total
invoices_cancelled_total

reports_generated_total
reports_failed_total
report_generation_duration_ms

notifications_sent_total
notifications_failed_total

storage_uploads_total
storage_downloads_total
storage_operation_duration_ms
```

---

# Buenas prácticas

## Sí

Usar logs estructurados:

```csharp
logger.LogInformation(
    "Client {ClientId} created",
    clientId);
```

Usar correlation id.

Usar traces para operaciones importantes.

Usar métricas para procesos críticos.

Usar health checks.

No loguear secretos.

Propagar correlation id en eventos.

---

## No

No usar:

```csharp
Console.WriteLine()
```

No concatenar logs:

```csharp
logger.LogInformation("Client " + clientId + " created");
```

No loguear:

```text
passwords
tokens
api keys
secret keys
tarjetas
datos sensibles innecesarios
```

No crear métricas con demasiadas combinaciones de tags.

---

# Errores comunes

## No propagar correlation id

Problema:

```text
API tiene corr-123
Evento no tiene corr-123
Consumer no se puede relacionar
```

Solución:

```text
incluir correlation id en EventMetadata
```

---

## Logs sin contexto

Incorrecto:

```csharp
logger.LogInformation("Created");
```

Correcto:

```csharp
logger.LogInformation(
    "Client {ClientId} created by user {UserId}",
    clientId,
    userId);
```

---

## Métricas con tags infinitos

Incorrecto:

```csharp
metric.Add(1, new("email", email));
```

Correcto:

```csharp
metric.Add(1, new("status", "success"));
```

No usar como tags:

```text
email
name
description
token
long text
unique ids masivos
```

---

## Health check sin dependencias

Incorrecto:

```text
/health responde OK aunque PostgreSQL está caído
```

Correcto:

```text
/health valida PostgreSQL, Redis, Mongo, etc.
```

---

# Flujo recomendado

```text
HTTP Request
    ↓
Correlation ID
    ↓
Request Logging
    ↓
Tracing
    ↓
CQRS Handler
    ↓
Metrics
    ↓
Logs
    ↓
Health Checks
```

---

# Resumen

`CustomCodeFramework.Observability` debe usarse para:

```text
logging estructurado
correlation id
distributed tracing
metrics
health checks
OpenTelemetry
Serilog
diagnóstico de errores
monitoreo de performance
propagación de contexto
```

Combinación recomendada:

```text
Api -> request pipeline
CQRS -> behaviors observables
Messaging -> propagation de correlation id
Redis/Postgres/Mongo -> health checks
Reports/Notifications/Storage -> métricas de negocio
```
