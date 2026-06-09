# CustomCodeFramework.Auth

## Introducción

`CustomCodeFramework.Auth` es el paquete encargado de centralizar autenticación, autorización, JWT, API Keys, claims, usuario actual y permisos dentro de CustomCodeFramework.

Este paquete busca que todos los servicios manejen seguridad de forma consistente.

Responsabilidades principales:

- Autenticación JWT.
- Autenticación por API Key.
- Generación de tokens.
- Lectura del usuario actual.
- Claims estándar.
- Extensiones para `ClaimsPrincipal`.
- Permisos dinámicos.
- Policies por permiso.
- Authorization handlers.
- Permission checker.
- Integración con ASP.NET Core Authorization.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Auth --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Auth": {
    "Jwt": {
      "Issuer": "customcode",
      "Audience": "customcode-api",
      "SecretKey": "CHANGE_THIS_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS",
      "ExpirationMinutes": 60,
      "RequireHttpsMetadata": true
    },
    "ApiKeys": {
      "HeaderName": "X-Api-Key",
      "Keys": {
        "internal-service": "CHANGE_THIS_API_KEY"
      }
    }
  }
}
```

---

# Registro en DI

## Solo JWT

```csharp
using CustomCodeFramework.Auth.DependencyInjection;

builder.Services.AddCustomCodeAuth(
    builder.Configuration,
    addJwt: true,
    addApiKeys: false);
```

---

## JWT + API Keys

```csharp
using CustomCodeFramework.Auth.DependencyInjection;

builder.Services.AddCustomCodeAuth(
    builder.Configuration,
    addJwt: true,
    addApiKeys: true);
```

---

## Middleware

```csharp
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

El orden correcto es:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Nunca al revés.

---

# Estructura

```text
CustomCodeFramework.Auth
├── Abstractions
│   ├── ICurrentUserService.cs
│   ├── IPermissionChecker.cs
│   └── ITokenService.cs
│
├── Claims
│   ├── CustomClaimTypes.cs
│   └── ClaimsPrincipalExtensions.cs
│
├── Permissions
│   ├── PermissionRequirement.cs
│   ├── PermissionAuthorizationHandler.cs
│   └── PermissionPolicyProvider.cs
│
├── Jwt
│   ├── JwtOptions.cs
│   ├── JwtTokenService.cs
│   └── JwtExtensions.cs
│
├── ApiKeys
│   ├── ApiKeyOptions.cs
│   └── ApiKeyAuthenticationHandler.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# Conceptos principales

## Authentication

Responde la pregunta:

```text
¿Quién es el usuario?
```

Ejemplos:

```text
JWT válido
API Key válida
Cookie válida
```

---

## Authorization

Responde la pregunta:

```text
¿Qué puede hacer el usuario?
```

Ejemplos:

```text
clients.create
clients.read
sales_orders.approve
reports.export
```

---

# JWT

JWT se usa principalmente para usuarios autenticados.

Flujo:

```text
Login
    ↓
Validar credenciales
    ↓
Generar JWT
    ↓
Cliente guarda token
    ↓
Cliente envía Authorization: Bearer token
    ↓
API valida token
    ↓
API obtiene claims
```

---

# JwtOptions

Configuración:

```json
{
  "Auth": {
    "Jwt": {
      "Issuer": "customcode",
      "Audience": "customcode-api",
      "SecretKey": "CHANGE_THIS_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS",
      "ExpirationMinutes": 60,
      "RequireHttpsMetadata": true
    }
  }
}
```

---

# ITokenService

## ¿Para qué sirve?

Genera tokens JWT.

---

## Login request

```csharp
public sealed record LoginRequest(
    string Email,
    string Password);
```

---

## Login response

```csharp
public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc);
```

---

## Ejemplo de login

```csharp
using CustomCodeFramework.Auth.Abstractions;

app.MapPost("/auth/login", async (
    LoginRequest request,
    ITokenService tokenService,
    CancellationToken cancellationToken) =>
{
    // Aquí se validan credenciales contra base de datos.
    // Este ejemplo asume que el usuario es válido.

    var token = tokenService.CreateToken(new TokenRequest
    {
        UserId = "user-001",
        UserName = "maurice",
        Email = request.Email,
        Roles = ["admin"],
        Permissions =
        [
            "clients.create",
            "clients.read",
            "clients.update",
            "clients.delete"
        ]
    });

    return Results.Ok(new LoginResponse(
        token.AccessToken,
        token.ExpiresAtUtc));
});
```

---

# Authorization header

El cliente debe enviar:

```http
Authorization: Bearer {access_token}
```

Ejemplo:

```bash
curl https://localhost:5001/clients \
  -H "Authorization: Bearer TOKEN"
```

---

# Claims

Los claims representan información del usuario dentro del token.

Ejemplos:

```text
sub
name
email
role
permission
tenant_id
```

---

# CustomClaimTypes

## ¿Para qué sirve?

Centraliza nombres de claims usados por el framework.

Ejemplo conceptual:

```csharp
CustomClaimTypes.UserId
CustomClaimTypes.Email
CustomClaimTypes.Permission
CustomClaimTypes.TenantId
```

---

# ClaimsPrincipalExtensions

## ¿Para qué sirve?

Permite leer claims de forma más limpia.

---

## Obtener UserId

```csharp
using CustomCodeFramework.Auth.Claims;

var userId = User.GetUserId();
```

---

## Obtener email

```csharp
using CustomCodeFramework.Auth.Claims;

var email = User.GetEmail();
```

---

## Obtener permisos

```csharp
using CustomCodeFramework.Auth.Claims;

var permissions = User.GetPermissions();
```

---

## Validar permiso

```csharp
using CustomCodeFramework.Auth.Claims;

if (!User.HasPermission("clients.create"))
{
    return Results.Forbid();
}
```

---

# ICurrentUserService

## ¿Para qué sirve?

Permite acceder al usuario actual sin depender directamente de `HttpContext`.

Esto evita usar `IHttpContextAccessor` por todo el código.

---

## Uso

```csharp
using CustomCodeFramework.Auth.Abstractions;

public sealed class AuditService(ICurrentUserService currentUser)
{
    public string GetCurrentUserId()
    {
        return currentUser.UserId ?? "system";
    }

    public bool IsAuthenticated()
    {
        return currentUser.IsAuthenticated;
    }
}
```

---

## Uso en CommandHandler

```csharp
using CustomCodeFramework.Auth.Abstractions;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;

public sealed class CreateClientCommandHandler(
    IRepository<Client, Guid> repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
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

        client.SetCreatedBy(
            currentUser.UserId ?? "system");

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

# API Keys

API Keys se usan para comunicación entre sistemas o servicios internos.

Ejemplos:

```text
report worker -> reports api
notification worker -> notifications api
external integration -> webhook api
```

No deben usarse como login de usuario final.

---

# ApiKeyOptions

Configuración:

```json
{
  "Auth": {
    "ApiKeys": {
      "HeaderName": "X-Api-Key",
      "Keys": {
        "internal-service": "CHANGE_THIS_API_KEY",
        "report-worker": "CHANGE_THIS_REPORT_WORKER_KEY"
      }
    }
  }
}
```

---

# Enviar API Key

```bash
curl https://localhost:5001/internal/process \
  -H "X-Api-Key: CHANGE_THIS_API_KEY"
```

---

# Endpoint protegido por API Key

Dependiendo de la configuración del paquete, el esquema de autenticación puede proteger endpoints internos.

Ejemplo conceptual:

```csharp
app.MapPost("/internal/reports/process", () =>
{
    return Results.Ok();
})
.RequireAuthorization();
```

---

# IPermissionChecker

## ¿Para qué sirve?

Permite validar permisos desde servicios o handlers.

---

## Uso

```csharp
using CustomCodeFramework.Auth.Abstractions;

public sealed class ClientPermissionService(
    IPermissionChecker permissionChecker)
{
    public async Task EnsureCanCreateAsync(
        CancellationToken cancellationToken)
    {
        var hasPermission = await permissionChecker.HasPermissionAsync(
            "clients.create",
            cancellationToken);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException();
        }
    }
}
```

---

# Permisos dinámicos

El framework está pensado para permisos tipo scope.

Ejemplos:

```text
clients.create
clients.read
clients.update
clients.delete

sales_orders.create
sales_orders.read
sales_orders.approve
sales_orders.cancel

reports.generate
reports.export
reports.download

storage.upload
storage.download
storage.delete

notifications.send
notifications.read

users.create
users.assign_roles
roles.create
permissions.assign
```

---

# PermissionRequirement

Representa un permiso requerido.

Uso conceptual:

```csharp
new PermissionRequirement("clients.create");
```

---

# PermissionAuthorizationHandler

## ¿Para qué sirve?

Valida si el usuario tiene el permiso requerido.

Flujo:

```text
Endpoint protegido
    ↓
PermissionRequirement
    ↓
PermissionAuthorizationHandler
    ↓
Claims del usuario
    ↓
Permitido / Denegado
```

---

# PermissionPolicyProvider

## ¿Para qué sirve?

Crea policies dinámicas basadas en permisos.

Sin tener que registrar manualmente:

```csharp
options.AddPolicy("clients.create", ...)
options.AddPolicy("clients.read", ...)
options.AddPolicy("clients.update", ...)
```

---

## Uso

```csharp
using CustomCodeFramework.Auth.Permissions;

app.MapPost("/clients", () =>
{
    return Results.Ok();
})
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.create"));
```

---

# Ejemplo real: proteger CRUD de clientes

```csharp
using CustomCodeFramework.Auth.Permissions;
using CustomCodeFramework.Cqrs.Dispatching;

var clients = app.MapGroup("/api/v1/clients")
    .WithTags("Clients");

clients.MapPost("/", async (
    CreateClientRequest request,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    var clientId = await commandDispatcher.DispatchAsync(
        new CreateClientCommand(
            request.Name,
            request.Email),
        cancellationToken);

    return Results.Created(
        $"/api/v1/clients/{clientId}",
        new
        {
            ClientId = clientId
        });
})
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.create"));

clients.MapGet("/{clientId:guid}", async (
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
})
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.read"));

clients.MapPut("/{clientId:guid}", async (
    Guid clientId,
    UpdateClientRequest request,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    await commandDispatcher.DispatchAsync(
        new UpdateClientCommand(
            clientId,
            request.Name,
            request.Email),
        cancellationToken);

    return Results.NoContent();
})
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.update"));

clients.MapDelete("/{clientId:guid}", async (
    Guid clientId,
    ICommandDispatcher commandDispatcher,
    CancellationToken cancellationToken) =>
{
    await commandDispatcher.DispatchAsync(
        new DeleteClientCommand(clientId),
        cancellationToken);

    return Results.NoContent();
})
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.delete"));
```

---

# Ejemplo real: permisos en Controller

```csharp
using CustomCodeFramework.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/clients")]
public sealed class ClientsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "permission:clients.create")]
    public IActionResult Create()
    {
        return Ok();
    }

    [HttpGet("{clientId:guid}")]
    [Authorize(Policy = "permission:clients.read")]
    public IActionResult GetById(Guid clientId)
    {
        return Ok();
    }
}
```

Si usás `BuildPolicyName`:

```csharp
[Authorize(Policy = PermissionPolicyProvider.BuildPolicyName("clients.create"))]
```

---

# Ejemplo real: generar token con permisos

```csharp
using CustomCodeFramework.Auth.Abstractions;

public sealed class LoginService(
    ITokenService tokenService)
{
    public LoginResponse Login(
        User user,
        IReadOnlyCollection<string> permissions)
    {
        var token = tokenService.CreateToken(new TokenRequest
        {
            UserId = user.Id.ToString(),
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.Roles.Select(role => role.Name).ToArray(),
            Permissions = permissions.ToArray(),
            Claims = new Dictionary<string, string>
            {
                ["tenant_id"] = user.TenantId.ToString()
            }
        });

        return new LoginResponse(
            token.AccessToken,
            token.ExpiresAtUtc);
    }
}
```

---

# Ejemplo payload de JWT

```json
{
  "sub": "user-001",
  "name": "maurice",
  "email": "maurice@example.com",
  "role": "admin",
  "permission": [
    "clients.create",
    "clients.read",
    "clients.update",
    "clients.delete"
  ],
  "tenant_id": "tenant-001",
  "iss": "customcode",
  "aud": "customcode-api",
  "exp": 1781020000
}
```

---

# Roles vs Permisos

## Rol

Agrupa permisos.

Ejemplos:

```text
admin
manager
seller
viewer
```

---

## Permiso

Autoriza una acción específica.

Ejemplos:

```text
clients.create
clients.read
clients.update
clients.delete
```

---

# Recomendación

El sistema debe autorizar por permisos, no solo por roles.

Correcto:

```csharp
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName("clients.create"));
```

Menos flexible:

```csharp
.RequireAuthorization("admin");
```

---

# Roles seed recomendados

Según el estándar del framework:

```text
superuser
administrator
```

Todos los demás roles deben poder ser creados por el sistema.

---

# Ejemplo de modelo de permisos

```csharp
public sealed class Permission
{
    public Guid PermissionId { get; init; }

    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string Module { get; init; } = default!;
}
```

---

# Ejemplo de permisos

```csharp
public static class ClientPermissions
{
    public const string Create = "clients.create";

    public const string Read = "clients.read";

    public const string Update = "clients.update";

    public const string Delete = "clients.delete";
}
```

Uso:

```csharp
.RequireAuthorization(
    PermissionPolicyProvider.BuildPolicyName(ClientPermissions.Create));
```

---

# Permisos por módulo

## Clients

```text
clients.create
clients.read
clients.update
clients.delete
```

## Sales Orders

```text
sales_orders.create
sales_orders.read
sales_orders.update
sales_orders.approve
sales_orders.cancel
```

## Reports

```text
reports.generate
reports.export
reports.download
```

## Storage

```text
storage.upload
storage.download
storage.delete
```

## Notifications

```text
notifications.send
notifications.read
```

## Users

```text
users.create
users.read
users.update
users.delete
users.assign_roles
```

## Roles

```text
roles.create
roles.read
roles.update
roles.delete
roles.assign_permissions
```

---

# Integración con CQRS

## Proteger endpoint

```text
Endpoint
    ↓
Authorization
    ↓
CommandDispatcher
    ↓
CommandHandler
```

Ejemplo:

```csharp
app.MapPost("/clients", CreateClientAsync)
    .RequireAuthorization(
        PermissionPolicyProvider.BuildPolicyName("clients.create"));
```

---

## Validar permiso dentro de handler

Esto debe usarse solo cuando la autorización depende de datos internos.

Ejemplo:

```csharp
using CustomCodeFramework.Auth.Abstractions;
using CustomCodeFramework.Cqrs.Commands;

public sealed class ApproveSalesOrderCommandHandler(
    IPermissionChecker permissionChecker)
    : ICommandHandler<ApproveSalesOrderCommand>
{
    public async Task HandleAsync(
        ApproveSalesOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var canApprove = await permissionChecker.HasPermissionAsync(
            "sales_orders.approve",
            cancellationToken);

        if (!canApprove)
        {
            throw new UnauthorizedAccessException();
        }

        // Aprobar orden.
    }
}
```

---

# Integración con Api

`CustomCodeFramework.Api` usa current user y correlation id.

Flujo:

```text
JWT/API Key
    ↓
Authentication
    ↓
ClaimsPrincipal
    ↓
CurrentUserMiddleware
    ↓
ICurrentUser / ICurrentUserService
```

---

# Integración con Observability

Agregar user id a logs:

```text
UserId
CorrelationId
TenantId
```

Ejemplo conceptual:

```csharp
logger.LogInformation(
    "User {UserId} created client {ClientId}",
    currentUser.UserId,
    clientId);
```

---

# Integración con Redis

Redis puede cachear permisos de usuario.

## Key

```csharp
var key = keyBuilder.Build(
    "auth",
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
            "auth",
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
            "auth",
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
            "auth",
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

# Program.cs completo

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Auth API",
    version: "v1");

builder.Services.AddCustomCodeAuth(
    builder.Configuration,
    addJwt: true,
    addApiKeys: true);

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "Auth.Api",
    serviceVersion: "0.1.0");

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

# Ejemplo completo: Auth endpoints

```csharp
using CustomCodeFramework.Auth.Abstractions;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/login", LoginAsync);

        return app;
    }

    private static Task<IResult> LoginAsync(
        LoginRequest request,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        // Validar usuario y contraseña contra base de datos.
        // Este ejemplo es simplificado.

        if (request.Email != "admin@example.com" ||
            request.Password != "admin")
        {
            return Task.FromResult<IResult>(
                Results.Unauthorized());
        }

        var token = tokenService.CreateToken(new TokenRequest
        {
            UserId = "user-001",
            UserName = "admin",
            Email = request.Email,
            Roles = ["administrator"],
            Permissions =
            [
                "clients.create",
                "clients.read",
                "clients.update",
                "clients.delete",
                "reports.generate",
                "reports.export"
            ]
        });

        return Task.FromResult<IResult>(
            Results.Ok(new LoginResponse(
                token.AccessToken,
                token.ExpiresAtUtc)));
    }
}

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc);
```

Program.cs:

```csharp
app.MapAuthEndpoints();
```

---

# Swagger con JWT

Después de registrar:

```csharp
builder.Services.AddCustomCodeApiWithSwagger(
    title: "Auth API",
    version: "v1");
```

Swagger debe permitir usar:

```text
Authorize
Bearer token
```

Formato:

```text
Bearer eyJhbGciOi...
```

---

# Seguridad recomendada

## SecretKey

Debe tener mínimo 32 caracteres.

Incorrecto:

```json
"SecretKey": "secret"
```

Correcto:

```json
"SecretKey": "CHANGE_THIS_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS"
```

---

# Variables de entorno

No guardar secretos reales en `appsettings.json`.

Usar variables de entorno:

```bash
export Auth__Jwt__SecretKey="super-secret-key-with-at-least-32-characters"
export Auth__ApiKeys__Keys__internal-service="internal-api-key"
```

---

# Buenas prácticas

## Sí

Usar permisos:

```text
clients.create
clients.read
```

Usar JWT para usuarios.

Usar API Keys para servicios internos.

Usar `ICurrentUserService`.

Usar `PermissionPolicyProvider`.

Cachear permisos con Redis si se consultan de base de datos.

Usar HTTPS.

Rotar API keys.

---

## No

No guardar secretos en Git.

No usar API Keys para usuarios finales.

No autorizar solo por rol si el sistema requiere permisos finos.

No usar JWT sin expiración.

No poner permisos hardcodeados dispersos sin constantes.

No usar `HttpContext.User` en todos los servicios.

---

# Errores comunes

## Middleware en orden incorrecto

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

## Token sin permisos

Problema:

```json
{
  "sub": "user-001",
  "email": "admin@example.com"
}
```

Endpoint requiere:

```text
clients.create
```

Resultado:

```text
403 Forbidden
```

Solución:

```json
{
  "permission": ["clients.create"]
}
```

---

## SecretKey muy corta

Problema:

```json
"SecretKey": "123"
```

Solución:

```json
"SecretKey": "CHANGE_THIS_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS"
```

---

## No usar HTTPS

En producción:

```json
"RequireHttpsMetadata": true
```

---

## Permisos inconsistentes

Incorrecto:

```text
client.create
clients_create
CLIENT_CREATE
```

Correcto:

```text
clients.create
```

---

# Flujo recomendado de autenticación

```text
POST /auth/login
    ↓
validar credenciales
    ↓
cargar roles
    ↓
cargar permisos
    ↓
generar JWT
    ↓
devolver access token
```

---

# Flujo recomendado de autorización

```text
Request con Bearer token
    ↓
Authentication middleware
    ↓
ClaimsPrincipal
    ↓
Authorization middleware
    ↓
PermissionRequirement
    ↓
PermissionAuthorizationHandler
    ↓
Endpoint
```

---

# Resumen

`CustomCodeFramework.Auth` debe usarse para:

```text
generar JWT
validar JWT
usar API Keys
leer usuario actual
manejar claims
proteger endpoints con permisos
crear policies dinámicas
validar permisos desde handlers
mantener autorización consistente
```

Combinación recomendada:

```text
Api -> middleware y endpoints
Auth -> autenticación/autorización
Redis -> cache de permisos
Observability -> logs con user id
CQRS -> handlers protegidos
```
