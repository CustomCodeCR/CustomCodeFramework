# CustomCodeFramework

Framework modular para acelerar el desarrollo de aplicaciones .NET utilizando CQRS, Clean Architecture, DDD, PostgreSQL, MongoDB, Redis, Storage, Reportes, Notificaciones, Seguridad y Observabilidad.

## Versión actual

```bash
0.1.0-alpha.1
```

---

# Configuración de NuGet

## Source local

```bash
dotnet nuget add source /home/maulang/Sistemas/CustomCodeFramework/artifacts/packages \
  --name customcode-local
```

Instalar desde source local:

```bash
dotnet add package CustomCodeFramework.Core \
  --version 0.1.0-alpha.1 \
  --source /home/maulang/Sistemas/CustomCodeFramework/artifacts/packages
```

## GitHub Packages

```bash
dotnet nuget add source "https://nuget.pkg.github.com/CustomCodeCR/index.json" \
  --name github \
  --username TU_USUARIO \
  --password TU_GITHUB_TOKEN \
  --store-password-in-clear-text
```

Instalar desde GitHub Packages:

```bash
dotnet add package CustomCodeFramework.Core \
  --version 0.1.0-alpha.1 \
  --source github
```

---

# Paquetes

## Core

### CustomCodeFramework.Core

Paquete base del framework.

### Incluye

- AggregateRoot
- Entity
- AuditableEntity
- SoftDeletableEntity
- Domain Events
- Value Objects
- Strongly Typed Ids
- Result Pattern
- Error Pattern
- Pagination
- Guards
- Common Extensions
- Shared Abstractions

### Instalación

```bash
dotnet add package CustomCodeFramework.Core --version 0.1.0-alpha.1
```

### Ejemplo

```csharp
using CustomCodeFramework.Core.Results;

var result = Result.Success();

var error = Error.Create(
    "clients.not_found",
    "Client was not found.");
```

### Entidad

```csharp
public sealed class Client : AuditableEntity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    private Client()
    {
    }

    public Client(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }
}
```

---

## Validation

### CustomCodeFramework.Validation

Validaciones basadas en FluentValidation.

### Incluye

- Validation Contracts
- Validation Errors
- ValidationException
- FluentValidation Integration
- Common Rules
- Result Mapping

### Instalación

```bash
dotnet add package CustomCodeFramework.Validation --version 0.1.0-alpha.1
```

### Registro

```csharp
builder.Services.AddCustomCodeValidation();
```

### Ejemplo

```csharp
public sealed record CreateClientRequest(
    string Name,
    string Email);

public sealed class CreateClientRequestValidator
    : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredString();

        RuleFor(x => x.Email)
            .RequiredString()
            .EmailAddress();
    }
}
```

---

## CQRS

### CustomCodeFramework.Cqrs

Implementación CQRS sin MediatR.

### Incluye

- Commands
- Queries
- Handlers
- Dispatchers
- Pipeline Behaviors
- Logging Behavior
- Validation Behavior
- Transaction Behavior
- Exception Behavior

### Instalación

```bash
dotnet add package CustomCodeFramework.Cqrs --version 0.1.0-alpha.1
```

### Registro

```csharp
builder.Services.AddCustomCodeCqrs();
```

### Command

```csharp
public sealed record CreateClientCommand(
    string Name,
    string Email)
    : ICommand<Guid>;
```

### Command Handler

```csharp
public sealed class CreateClientCommandHandler
    : ICommandHandler<CreateClientCommand, Guid>
{
    public Task<Guid> HandleAsync(
        CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}
```

### Query

```csharp
public sealed record GetClientByIdQuery(Guid ClientId)
    : IQuery<ClientResponse?>;
```

### Query Handler

```csharp
public sealed class GetClientByIdQueryHandler
    : IQueryHandler<GetClientByIdQuery, ClientResponse?>
{
    public Task<ClientResponse?> HandleAsync(
        GetClientByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ClientResponse?>(
            new ClientResponse(
                query.ClientId,
                "ACME",
                "client@example.com"));
    }
}
```

---

## Persistence

### CustomCodeFramework.Persistence

Abstracciones de persistencia.

### Incluye

- IUnitOfWork
- IRepository
- IReadRepository
- ITransactionManager
- IConnectionFactory
- Specifications
- TransactionResult
- TransactionScopeOptions

### Instalación

```bash
dotnet add package CustomCodeFramework.Persistence --version 0.1.0-alpha.1
```

---

## API

### CustomCodeFramework.Api

Extensiones para ASP.NET Core.

### Incluye

- Global Exception Handler
- ProblemDetails
- CorrelationId Middleware
- Request Logging
- Current User Middleware
- Standard Responses
- Endpoint Filters
- Swagger
- API Versioning

### Instalación

```bash
dotnet add package CustomCodeFramework.Api --version 0.1.0-alpha.1
```

### Registro

```csharp
builder.Services.AddCustomCodeApiWithSwagger(
    title: "My API",
    version: "v1");
```

---

## Auth

### CustomCodeFramework.Auth

Autenticación y autorización.

### Incluye

- JWT
- API Keys
- Current User Service
- Permission System
- Permission Policies
- Permission Authorization Handler
- Token Service

### Instalación

```bash
dotnet add package CustomCodeFramework.Auth --version 0.1.0-alpha.1
```

### Registro

```csharp
builder.Services.AddCustomCodeAuth(
    builder.Configuration,
    addJwt: true,
    addApiKeys: true);
```

---

## Observability

### CustomCodeFramework.Observability

Observabilidad basada en OpenTelemetry.

### Incluye

- Serilog
- Structured Logging
- Tracing
- Metrics
- Health Checks
- Correlation Context

### Instalación

```bash
dotnet add package CustomCodeFramework.Observability --version 0.1.0-alpha.1
```

---

# Bases de Datos

## PostgreSQL

### CustomCodeFramework.Postgres

- Connection Factory
- Retry Policies
- Health Checks
- Script Runner

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
```

---

### CustomCodeFramework.Postgres.EntityFramework

Incluye:

- DbContext Base
- Repositories
- UnitOfWork
- Outbox
- Inbox
- Interceptors
- Transactions

```bash
dotnet add package CustomCodeFramework.Postgres.EntityFramework --version 0.1.0-alpha.1
```

---

### CustomCodeFramework.Postgres.Dapper

Incluye:

- Query Executor
- Command Executor
- Pagination Builder
- Transaction Context
- Type Handlers

```bash
dotnet add package CustomCodeFramework.Postgres.Dapper --version 0.1.0-alpha.1
```

---

## MongoDB

### CustomCodeFramework.Mongo

Incluye:

- Mongo Context
- Collection Provider
- Read Models
- Projection Store
- Transactions
- Health Checks

```bash
dotnet add package CustomCodeFramework.Mongo --version 0.1.0-alpha.1
```

---

## Redis

### CustomCodeFramework.Redis

Incluye:

- Cache Service
- Distributed Locks
- Key Builder
- Serializer
- Health Checks

```bash
dotnet add package CustomCodeFramework.Redis --version 0.1.0-alpha.1
```

---

# Messaging

## CustomCodeFramework.Messaging

### Incluye

- Integration Events
- Event Bus Abstractions
- Event Metadata
- Event Headers
- Outbox Contracts
- Inbox Contracts
- JSON Serialization

```bash
dotnet add package CustomCodeFramework.Messaging --version 0.1.0-alpha.1
```

---

# Storage

## CustomCodeFramework.Storage

### Incluye

- Upload
- Download
- Metadata
- Validation
- Signed URLs
- Permissions
- Visibility

```bash
dotnet add package CustomCodeFramework.Storage --version 0.1.0-alpha.1
```

### Providers disponibles

- CustomCodeFramework.Storage.Local
- CustomCodeFramework.Storage.S3
- CustomCodeFramework.Storage.AzureBlob
- CustomCodeFramework.Storage.GoogleCloud
- CustomCodeFramework.Storage.Firebase

---

# Reports

## CustomCodeFramework.Reports

### Incluye

- Report Definitions
- Report Requests
- Report Results
- Report Formats
- Templates
- Storage Contracts
- Report Generator

```bash
dotnet add package CustomCodeFramework.Reports --version 0.1.0-alpha.1
```

### Exportadores

```bash
CustomCodeFramework.Reports.Csv
CustomCodeFramework.Reports.Excel
CustomCodeFramework.Reports.Word
CustomCodeFramework.Reports.Pdf
```

---

# Notifications

## CustomCodeFramework.Notifications

### Incluye

- Notification Message
- Recipient
- Attachments
- Templates
- Preferences
- Outbox
- Dispatcher

```bash
dotnet add package CustomCodeFramework.Notifications --version 0.1.0-alpha.1
```

### Providers disponibles

```bash
CustomCodeFramework.Notifications.Email
CustomCodeFramework.Notifications.SignalR
CustomCodeFramework.Notifications.Sms
CustomCodeFramework.Notifications.WhatsApp
```

---

# Instalación recomendada por tipo de servicio

## API básica

```bash
dotnet add package CustomCodeFramework.Core
dotnet add package CustomCodeFramework.Validation
dotnet add package CustomCodeFramework.Cqrs
dotnet add package CustomCodeFramework.Persistence
dotnet add package CustomCodeFramework.Api
dotnet add package CustomCodeFramework.Auth
dotnet add package CustomCodeFramework.Observability
```

## API con PostgreSQL

```bash
dotnet add package CustomCodeFramework.Postgres
dotnet add package CustomCodeFramework.Postgres.EntityFramework
dotnet add package CustomCodeFramework.Postgres.Dapper
```

## API con MongoDB y Redis

```bash
dotnet add package CustomCodeFramework.Mongo
dotnet add package CustomCodeFramework.Redis
```

## API con Reportes

```bash
dotnet add package CustomCodeFramework.Reports
dotnet add package CustomCodeFramework.Reports.Csv
dotnet add package CustomCodeFramework.Reports.Excel
dotnet add package CustomCodeFramework.Reports.Word
dotnet add package CustomCodeFramework.Reports.Pdf
```

## API con Storage

```bash
dotnet add package CustomCodeFramework.Storage
dotnet add package CustomCodeFramework.Storage.Local
```

## API con Notificaciones

```bash
dotnet add package CustomCodeFramework.Notifications
dotnet add package CustomCodeFramework.Notifications.Email
dotnet add package CustomCodeFramework.Notifications.SignalR
dotnet add package CustomCodeFramework.Notifications.Sms
dotnet add package CustomCodeFramework.Notifications.WhatsApp
```

---

# Arquitectura recomendada

```text
Application
├── Commands
├── Queries
├── Behaviors
├── Validators

Domain
├── Entities
├── ValueObjects
├── Events
├── Repositories

Infrastructure
├── Persistence
├── Messaging
├── Storage
├── Notifications

Api
├── Endpoints
├── Middleware
├── Filters
├── Configuration
```

---

# Licencia

Copyright © CustomCodeCR

Todos los derechos reservados.
