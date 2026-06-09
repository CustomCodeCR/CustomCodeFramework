# CustomCodeFramework.Api

## Introducción

`CustomCodeFramework.Api` es el paquete encargado de centralizar utilidades comunes para APIs ASP.NET Core.

Este paquete busca que todas las APIs tengan el mismo estándar en:

- Manejo global de excepciones.
- ProblemDetails.
- Respuestas HTTP.
- Respuestas paginadas.
- Middleware de correlation id.
- Middleware de logging.
- Middleware de current user.
- Endpoint filters.
- Swagger/OpenAPI.
- Versionado de API.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Api --version 0.1.0-alpha.1
```

---

# Configuración recomendada

```json
{
  "Api": {
    "ApplicationName": "Clients API",
    "EnableDetailedErrors": false,
    "CorrelationIdHeaderName": "X-Correlation-Id"
  },
  "Swagger": {
    "Title": "Clients API",
    "Version": "v1",
    "Description": "Clients service API."
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Api.DependencyInjection;

builder.Services.AddCustomCodeApi(builder.Configuration);
```

Con Swagger:

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");
```

---

# Estructura

```text
CustomCodeFramework.Api
├── Exceptions
│   ├── GlobalExceptionHandler.cs
│   └── ExceptionMappingOptions.cs
│
├── ProblemDetails
│   ├── ProblemDetailsFactory.cs
│   └── ProblemDetailsExtensions.cs
│
├── Middleware
│   ├── CorrelationIdMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── CurrentUserMiddleware.cs
│
├── Responses
│   ├── ApiResponse.cs
│   ├── ApiErrorResponse.cs
│   └── ApiPagedResponse.cs
│
├── EndpointFilters
│   ├── ValidationEndpointFilter.cs
│   └── TransactionEndpointFilter.cs
│
├── Swagger
│   └── SwaggerExtensions.cs
│
├── Versioning
│   └── ApiVersioningExtensions.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# Program.cs básico

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

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

# Program.cs con Minimal APIs

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapGet("/clients", () =>
{
    return Results.Ok("Clients API");
});

app.Run();
```

---

# GlobalExceptionHandler

## ¿Para qué sirve?

Permite centralizar el manejo de errores.

Evita repetir esto en cada endpoint:

```csharp
try
{
    // ejecutar
}
catch (Exception exception)
{
    return Results.Problem(exception.Message);
}
```

---

# Flujo con exception handler

```text
Endpoint
    ↓
Handler
    ↓
Exception
    ↓
GlobalExceptionHandler
    ↓
ProblemDetails
    ↓
HTTP Response
```

---

# Ejemplo de error de validación

```csharp
throw new ValidationException(errors);
```

Respuesta esperada:

```json
{
  "type": "validation_error",
  "title": "Validation failed",
  "status": 400,
  "errors": [
    {
      "property": "Email",
      "message": "Email is required.",
      "code": "required"
    }
  ]
}
```

---

# Ejemplo de error de negocio

```csharp
throw new BusinessRuleException("Client already exists.");
```

Respuesta esperada:

```json
{
  "type": "business_rule_error",
  "title": "Business rule violation",
  "status": 400,
  "detail": "Client already exists."
}
```

---

# Ejemplo de error no controlado

```csharp
throw new InvalidOperationException("Unexpected error.");
```

En desarrollo:

```json
{
  "type": "internal_server_error",
  "title": "Internal server error",
  "status": 500,
  "detail": "Unexpected error."
}
```

En producción:

```json
{
  "type": "internal_server_error",
  "title": "Internal server error",
  "status": 500,
  "detail": "An unexpected error occurred."
}
```

---

# ExceptionMappingOptions

## ¿Para qué sirve?

Permite configurar cómo mapear excepciones a códigos HTTP.

Ejemplo conceptual:

```csharp
builder.Services.Configure<ExceptionMappingOptions>(options =>
{
    options.Map<BusinessRuleException>(StatusCodes.Status400BadRequest);
    options.Map<ValidationException>(StatusCodes.Status400BadRequest);
    options.Map<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized);
});
```

---

# ProblemDetails

## ¿Qué es?

`ProblemDetails` es un formato estándar para errores HTTP.

Ejemplo:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Client was not found.",
  "instance": "/clients/123"
}
```

---

# ProblemDetailsFactory

## ¿Para qué sirve?

Crea respuestas de error consistentes.

Uso conceptual:

```csharp
using CustomCodeFramework.Api.ProblemDetails;

var problem = problemDetailsFactory.Create(
    statusCode: 404,
    title: "Client not found",
    detail: "The requested client does not exist.");
```

---

# ProblemDetailsExtensions

## Uso

```csharp
return Results.Problem(
    title: "Client not found",
    statusCode: StatusCodes.Status404NotFound);
```

O mediante extensiones del framework:

```csharp
return problemDetails.ToResult();
```

---

# ApiResponse

## ¿Para qué sirve?

Estandariza respuestas exitosas.

---

## Ejemplo

```csharp
using CustomCodeFramework.Api.Responses;

return Results.Ok(
    ApiResponse<object>.Ok(new
    {
        ClientId = Guid.NewGuid(),
        Name = "ACME"
    }));
```

Respuesta:

```json
{
  "success": true,
  "data": {
    "clientId": "dceff61f-1234-4ef2-b4d9-83a1d7f3f64f",
    "name": "ACME"
  }
}
```

---

# ApiResponse con mensaje

```csharp
using CustomCodeFramework.Api.Responses;

return Results.Ok(
    ApiResponse<object>.Ok(
        data: new
        {
            ClientId = clientId
        },
        message: "Client created successfully."));
```

Respuesta:

```json
{
  "success": true,
  "message": "Client created successfully.",
  "data": {
    "clientId": "dceff61f-1234-4ef2-b4d9-83a1d7f3f64f"
  }
}
```

---

# ApiErrorResponse

## ¿Para qué sirve?

Estandariza respuestas de error cuando no se usa ProblemDetails.

Ejemplo:

```csharp
using CustomCodeFramework.Api.Responses;

return Results.BadRequest(
    ApiErrorResponse.Create(
        code: "clients.invalid_email",
        message: "Invalid email."));
```

Respuesta:

```json
{
  "success": false,
  "error": {
    "code": "clients.invalid_email",
    "message": "Invalid email."
  }
}
```

---

# ApiPagedResponse

## ¿Para qué sirve?

Estandariza respuestas paginadas.

---

## Ejemplo

```csharp
using CustomCodeFramework.Api.Responses;

var response = ApiPagedResponse<ClientResponse>.Ok(
    items: clients,
    pageNumber: 1,
    pageSize: 10,
    totalCount: 150);

return Results.Ok(response);
```

Respuesta:

```json
{
  "success": true,
  "data": [
    {
      "clientId": "dceff61f-1234-4ef2-b4d9-83a1d7f3f64f",
      "name": "ACME"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

# CorrelationIdMiddleware

## ¿Para qué sirve?

Permite rastrear una petición a través de logs, servicios y eventos.

Header recomendado:

```text
X-Correlation-Id
```

---

## Flujo

```text
Cliente HTTP
    ↓ X-Correlation-Id
API
    ↓ Logs
    ↓ Events
    ↓ Downstream services
```

---

## Request con correlation id

```bash
curl -H "X-Correlation-Id: corr-123" \
  https://localhost:5001/clients
```

---

## Si no viene header

El middleware genera uno nuevo.

```text
X-Correlation-Id: generated-guid
```

---

## Uso interno

```csharp
using CustomCodeFramework.Core.Abstractions;

public sealed class ClientService(ICorrelationContext correlationContext)
{
    public string GetCorrelationId()
    {
        return correlationContext.CorrelationId;
    }
}
```

---

# RequestLoggingMiddleware

## ¿Para qué sirve?

Registra información de cada request.

Datos típicos:

```text
method
path
status code
elapsed milliseconds
correlation id
user id
```

---

## Ejemplo de log

```text
HTTP POST /clients responded 201 in 125 ms. CorrelationId=corr-123
```

---

# CurrentUserMiddleware

## ¿Para qué sirve?

Extrae información del usuario actual desde claims y la deja disponible para la aplicación.

Ejemplo de claims:

```text
sub
name
email
role
permissions
```

Uso:

```csharp
using CustomCodeFramework.Core.Abstractions;

public sealed class AuditService(ICurrentUser currentUser)
{
    public string GetCreatedBy()
    {
        return currentUser.UserId ?? "system";
    }
}
```

---

# EndpointFilters

Los endpoint filters permiten ejecutar lógica alrededor de Minimal APIs.

---

# ValidationEndpointFilter

## ¿Para qué sirve?

Valida el request antes de ejecutar el endpoint.

---

## Ejemplo request

```csharp
public sealed record CreateClientRequest(
    string Name,
    string Email);
```

---

## Validator

```csharp
using FluentValidation;

public sealed class CreateClientRequestValidator
    : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

---

## Endpoint

```csharp
using CustomCodeFramework.Api.EndpointFilters;

app.MapPost("/clients", async (CreateClientRequest request) =>
{
    return Results.Ok();
})
.AddEndpointFilter<ValidationEndpointFilter<CreateClientRequest>>();
```

---

# TransactionEndpointFilter

## ¿Para qué sirve?

Permite ejecutar un endpoint dentro de una transacción.

Ejemplo conceptual:

```csharp
using CustomCodeFramework.Api.EndpointFilters;

app.MapPost("/sales-orders", async (CreateSalesOrderRequest request) =>
{
    return Results.Ok();
})
.AddEndpointFilter<TransactionEndpointFilter>();
```

Flujo:

```text
Request
    ↓
Begin transaction
    ↓
Endpoint
    ↓
Commit
```

Si falla:

```text
Request
    ↓
Begin transaction
    ↓
Exception
    ↓
Rollback
```

---

# SwaggerExtensions

## ¿Para qué sirve?

Centraliza configuración de Swagger/OpenAPI.

---

## Registro

```csharp
using CustomCodeFramework.Api.Swagger;

builder.Services.AddCustomCodeSwagger(
    title: "Clients API",
    version: "v1");
```

---

## Uso

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}
```

---

# Swagger con JWT

Cuando se usa Auth, Swagger debe permitir Bearer Token.

Ejemplo esperado:

```text
Authorize
    ↓
Bearer token
    ↓
Probar endpoints protegidos
```

Program.cs:

```csharp
builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");
```

---

# ApiVersioningExtensions

## ¿Para qué sirve?

Permite configurar versionado de API.

Ejemplo conceptual:

```csharp
using CustomCodeFramework.Api.Versioning;

builder.Services.AddCustomCodeApiVersioning();
```

---

## Versionado por URL

```text
/api/v1/clients
/api/v2/clients
```

---

## Versionado por header

```text
X-Api-Version: 1.0
```

---

# Ejemplo real: API de clientes con CQRS

## Request

```csharp
public sealed record CreateClientRequest(
    string Name,
    string Email,
    string PhoneNumber);
```

---

## Endpoint

```csharp
using CustomCodeFramework.Api.Responses;
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
        ApiResponse<object>.Ok(
            new
            {
                ClientId = clientId
            },
            "Client created successfully."));
});
```

---

# Ejemplo real: GET por ID

```csharp
using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Cqrs.Dispatching;

app.MapGet("/clients/{clientId:guid}", async (
    Guid clientId,
    IQueryDispatcher queryDispatcher,
    CancellationToken cancellationToken) =>
{
    var client = await queryDispatcher.DispatchAsync(
        new GetClientByIdQuery(clientId),
        cancellationToken);

    if (client is null)
    {
        return Results.NotFound(
            ApiErrorResponse.Create(
                "clients.not_found",
                "Client was not found."));
    }

    return Results.Ok(
        ApiResponse<ClientResponse>.Ok(client));
});
```

---

# Ejemplo real: GET paginado

```csharp
using CustomCodeFramework.Api.Responses;
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

    return Results.Ok(
        ApiPagedResponse<ClientResponse>.Ok(
            result.Items,
            result.PageNumber,
            result.PageSize,
            result.TotalCount));
});
```

---

# Ejemplo real: DELETE

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

# Ejemplo real: endpoint protegido

```csharp
using CustomCodeFramework.Auth.Permissions;
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
})
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.create"));
```

---

# Ejemplo con Controller

## Controller

```csharp
using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Cqrs.Dispatching;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/clients")]
public sealed class ClientsController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = await commandDispatcher.DispatchAsync(
            new CreateClientCommand(
                request.Name,
                request.Email,
                request.PhoneNumber),
            cancellationToken);

        return Created(
            $"/api/v1/clients/{clientId}",
            ApiResponse<object>.Ok(
                new
                {
                    ClientId = clientId
                }));
    }

    [HttpGet("{clientId:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken)
    {
        var client = await queryDispatcher.DispatchAsync(
            new GetClientByIdQuery(clientId),
            cancellationToken);

        if (client is null)
        {
            return NotFound(
                ApiErrorResponse.Create(
                    "clients.not_found",
                    "Client was not found."));
        }

        return Ok(ApiResponse<ClientResponse>.Ok(client));
    }
}
```

---

# Ejemplo: MapGroup

```csharp
var clients = app.MapGroup("/api/v1/clients")
    .WithTags("Clients");

clients.MapGet("/", async (
    IQueryDispatcher queryDispatcher,
    CancellationToken cancellationToken) =>
{
    var result = await queryDispatcher.DispatchAsync(
        new SearchClientsQuery(null, 1, 20),
        cancellationToken);

    return Results.Ok(result);
});

clients.MapPost("/", async (
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
        $"/api/v1/clients/{clientId}",
        new
        {
            ClientId = clientId
        });
});
```

---

# Ejemplo: Extension method por feature

## ClientsEndpoints.cs

```csharp
using CustomCodeFramework.Cqrs.Dispatching;

public static class ClientsEndpoints
{
    public static IEndpointRouteBuilder MapClientsEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/clients")
            .WithTags("Clients");

        group.MapPost("/", CreateAsync);
        group.MapGet("/{clientId:guid}", GetByIdAsync);
        group.MapDelete("/{clientId:guid}", DeleteAsync);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateClientRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var clientId = await commandDispatcher.DispatchAsync(
            new CreateClientCommand(
                request.Name,
                request.Email,
                request.PhoneNumber),
            cancellationToken);

        return Results.Created(
            $"/api/v1/clients/{clientId}",
            new
            {
                ClientId = clientId
            });
    }

    private static async Task<IResult> GetByIdAsync(
        Guid clientId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var client = await queryDispatcher.DispatchAsync(
            new GetClientByIdQuery(clientId),
            cancellationToken);

        return client is null
            ? Results.NotFound()
            : Results.Ok(client);
    }

    private static async Task<IResult> DeleteAsync(
        Guid clientId,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        await commandDispatcher.DispatchAsync(
            new DeleteClientCommand(clientId),
            cancellationToken);

        return Results.NoContent();
    }
}
```

Program.cs:

```csharp
app.MapClientsEndpoints();
```

---

# Ejemplo completo Program.cs con API + CQRS + Auth + Observability

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;
using CustomCodeFramework.Observability.HealthChecks;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Clients API",
    version: "v1");

builder.Services.AddCustomCodeAuth(
    builder.Configuration,
    addJwt: true,
    addApiKeys: false);

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomCodeHealthChecks("/health");

app.MapControllers();
app.MapClientsEndpoints();

app.Run();
```

---

# Orden recomendado de middleware

```csharp
app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

---

# Orden conceptual

```text
Exception handler
    ↓
Correlation id
    ↓
Request logging
    ↓
Current user
    ↓
Authentication
    ↓
Authorization
    ↓
Endpoint
```

---

# appsettings.json completo

```json
{
  "Api": {
    "ApplicationName": "Clients API",
    "EnableDetailedErrors": false,
    "CorrelationIdHeaderName": "X-Correlation-Id"
  },
  "Swagger": {
    "Title": "Clients API",
    "Version": "v1",
    "Description": "Clients service API."
  }
}
```

---

# Integración con CustomCodeFramework.Cqrs

```text
Endpoint
    ↓
CommandDispatcher / QueryDispatcher
    ↓
CQRS Behavior
    ↓
Handler
```

Ejemplo:

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

# Integración con CustomCodeFramework.Validation

```text
Endpoint
    ↓
ValidationEndpointFilter
    ↓
Validator
    ↓
Endpoint handler
```

O vía CQRS:

```text
Endpoint
    ↓
CommandDispatcher
    ↓
ValidationBehavior
    ↓
Validator
    ↓
CommandHandler
```

---

# Integración con CustomCodeFramework.Auth

```text
HTTP Request
    ↓
JWT Bearer
    ↓
Claims
    ↓
CurrentUserMiddleware
    ↓
ICurrentUser
```

Endpoint protegido:

```csharp
app.MapPost("/clients", CreateClientAsync)
    .RequireAuthorization(
        PermissionPolicyProvider.BuildPolicyName("clients.create"));
```

---

# Integración con Observability

```text
RequestLoggingMiddleware
    ↓
CorrelationId
    ↓
Logs
    ↓
Traces
```

---

# Organización recomendada

```text
Api
├── Program.cs
├── Endpoints
│   ├── ClientsEndpoints.cs
│   ├── SalesOrdersEndpoints.cs
│   └── ReportsEndpoints.cs
│
├── Requests
│   ├── CreateClientRequest.cs
│   └── UpdateClientRequest.cs
│
└── Responses
    └── ClientResponse.cs
```

O por feature:

```text
Features
└── Clients
    ├── CreateClient
    │   ├── CreateClientRequest.cs
    │   ├── CreateClientCommand.cs
    │   ├── CreateClientCommandHandler.cs
    │   ├── CreateClientCommandValidator.cs
    │   └── CreateClientEndpoint.cs
    │
    ├── GetClientById
    │   ├── GetClientByIdQuery.cs
    │   ├── GetClientByIdQueryHandler.cs
    │   └── GetClientByIdEndpoint.cs
    │
    └── ClientsEndpoints.cs
```

---

# Buenas prácticas

## Sí

Usar respuestas estándar:

```csharp
ApiResponse<T>
ApiPagedResponse<T>
ApiErrorResponse
```

Usar exception handler global.

Usar correlation id.

Usar endpoint groups.

Usar CQRS desde endpoints.

Mantener endpoints delgados.

---

## No

No meter lógica de negocio en endpoints.

Incorrecto:

```csharp
app.MapPost("/clients", async (CreateClientRequest request, AppDbContext db) =>
{
    if (request.Name == "")
    {
        return Results.BadRequest();
    }

    db.Clients.Add(...);

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

# Errores comunes

## No usar UseCustomCodeApi

Si no se registra:

```csharp
app.UseCustomCodeApi();
```

Puede que no funcionen:

```text
exception handler
correlation id
request logging
current user
```

---

## Poner Authorization antes de Authentication

Incorrecto:

```csharp
app.UseAuthorization();
app.UseAuthentication();
```

Correcto:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

---

## Endpoint con demasiada lógica

Incorrecto:

```csharp
app.MapPost("/clients", async (...) =>
{
    // validar
    // consultar DB
    // crear entidad
    // guardar
    // publicar evento
    // enviar email
});
```

Correcto:

```text
Endpoint -> Command -> Handler
```

---

## Devolver entidades de dominio

Incorrecto:

```csharp
return Results.Ok(clientEntity);
```

Correcto:

```csharp
return Results.Ok(clientResponse);
```

---

# Flujo recomendado de request

```text
HTTP Request
    ↓
GlobalExceptionHandler
    ↓
CorrelationIdMiddleware
    ↓
RequestLoggingMiddleware
    ↓
CurrentUserMiddleware
    ↓
Authentication
    ↓
Authorization
    ↓
Endpoint
    ↓
CQRS Dispatcher
    ↓
Handler
    ↓
ApiResponse / ProblemDetails
```

---

# Resumen

`CustomCodeFramework.Api` debe usarse para:

```text
estandarizar APIs
manejar excepciones globalmente
generar ProblemDetails
crear respuestas consistentes
agregar correlation id
registrar requests
leer usuario actual
usar filtros de endpoints
configurar Swagger
configurar versionado
```

No debe contener lógica de negocio.

La API debe ser la capa más delgada posible:

```text
HTTP -> Request DTO -> CQRS -> Response DTO
```
