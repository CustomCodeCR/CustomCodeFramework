# CustomCodeFramework changes for HU-001 to HU-004

This framework keeps the generic `CustomCodeFramework` name. The changes below add reusable infrastructure needed by Dhole and other future projects.

## Added projects

### CustomCodeFramework.Auth.Redis
Provides Redis-backed JWT security helpers:

- `ITokenRevocationStore`
- `ISessionValidationStore`
- `RedisTokenRevocationStore`
- `RedisSessionValidationStore`
- JWT bearer post-configuration for revoked-token and active-session validation

Configuration section:

```json
{
  "Auth": {
    "Redis": {
      "ValidateRevokedTokens": true,
      "ValidateActiveSessions": false,
      "DefaultRevocationExpirationMinutes": 1440,
      "DefaultSessionExpirationMinutes": 1440
    }
  }
}
```

### CustomCodeFramework.ServiceDefaults
Central bootstrap package for microservices.

Main extension:

```csharp
builder.Services.AddCustomCodeServiceDefaults(builder.Configuration);
```

It wires the common API, Swagger, Auth, Redis, Auth.Redis, Postgres, optional Mongo and optional Redis Streams registrations.

## Modified existing projects

### CustomCodeFramework.Core
`ICurrentUser` now supports distributed security details:

- `UserType`
- `SessionId`
- `TokenVersion`
- `Scopes`
- `HasScope`
- `HasPermission`

### CustomCodeFramework.Auth
JWT and current-user support now includes:

- `scope` claims
- `user_type`
- `token_version`
- `session_id`
- `RequireScopeAttribute`
- dynamic `Scope:` authorization policies
- scope authorization handler

Example:

```csharp
[RequireScope("crm.clients.view")]
public static IResult GetClients() => Results.Ok();
```

or minimal API:

```csharp
app.MapGet("/clients", Handler)
   .RequireAuthorization("Scope:crm.clients.view");
```

### CustomCodeFramework.Messaging
Messaging contracts were expanded for service independence and traceability:

- `IntegrationEvent` now has `CorrelationId`, `SourceService` and `Version`.
- `EventEnvelope` now carries `CorrelationId`, `SourceService`, `Version` and `OccurredAtUtc`.
- Added standard `OutboxMessage` and `InboxMessage` models.
- Added outbox/inbox statuses.

### CustomCodeFramework.Postgres.EntityFramework
Added real EF configurations for:

- `outbox_messages`
- `inbox_messages`

These include event id, event type, source service, payload json, headers json, correlation id, status, retry count and processing dates.

## Improved template

The `customcode-microservice` template now creates a ready service structure:

```txt
Company.Service.Api
Company.Service.Application
Company.Service.Domain
Company.Service.Infrastructure
Company.Service.Persistence
Company.Service.Contracts
Company.Service.Workers
Company.Service.UnitTests
Company.Service.IntegrationTests
```

The API template includes:

- service defaults
- JWT local validation
- scope-protected sample endpoint
- health check endpoint
- default Auth/Postgres/Redis/Mongo config sections

