# CustomCodeFramework.Core

## Introducción

`CustomCodeFramework.Core` es el paquete base de todo el framework.

Todos los demás paquetes dependen directa o indirectamente de este paquete.

Responsabilidades:

- Entidades base.
- Aggregate Roots.
- Domain Events.
- Value Objects.
- Strongly Typed IDs.
- Result Pattern.
- Errors.
- Excepciones de dominio.
- Auditoría.
- Soft Delete.
- Paginación.
- Guards.
- Abstracciones comunes.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Core
```

---

# Dependencias

Ninguna.

Es el paquete raíz del framework.

---

# Estructura

```text
CustomCodeFramework.Core
├── Abstractions
├── Domain
├── Results
├── Pagination
├── Guards
└── Extensions
```

---

# Result Pattern

## ¿Por qué existe?

Evita excepciones para errores de negocio.

Incorrecto:

```csharp
throw new Exception("Client not found");
```

Correcto:

```csharp
return Result.Failure(
    Error.Create(
        "clients.not_found",
        "Client not found"));
```

---

# Error

Representa un error de negocio.

## Creación

```csharp
using CustomCodeFramework.Core.Results;

var error = Error.Create(
    "clients.not_found",
    "Client not found");
```

Resultado:

```json
{
  "code": "clients.not_found",
  "message": "Client not found"
}
```

---

# Result

Operaciones sin retorno.

## Success

```csharp
return Result.Success();
```

---

## Failure

```csharp
return Result.Failure(
    Error.Create(
        "clients.not_found",
        "Client not found"));
```

---

## Ejemplo real

```csharp
public Result DeleteClient(Guid clientId)
{
    var exists = repository.Exists(clientId);

    if (!exists)
    {
        return Result.Failure(
            Error.Create(
                "clients.not_found",
                "Client not found"));
    }

    repository.Delete(clientId);

    return Result.Success();
}
```

---

# Result<T>

Operaciones con retorno.

## Success

```csharp
return Result.Success(client);
```

---

## Failure

```csharp
return Result.Failure<Client>(
    Error.Create(
        "clients.not_found",
        "Client not found"));
```

---

## Ejemplo real

```csharp
public Result<Client> GetClient(Guid clientId)
{
    var client = repository.Get(clientId);

    if (client is null)
    {
        return Result.Failure<Client>(
            Error.Create(
                "clients.not_found",
                "Client not found"));
    }

    return Result.Success(client);
}
```

---

# Entity

Entidad base del dominio.

Todas las entidades heredan de ella.

---

## Ejemplo

```csharp
public sealed class Client : Entity<Guid>
{
    public string Name { get; private set; }

    private Client()
    {
    }

    public Client(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

---

# AggregateRoot

Representa una raíz de agregado DDD.

Permite manejar Domain Events.

---

## Ejemplo

```csharp
public sealed class Client : AggregateRoot<Guid>
{
    public string Name { get; private set; }

    private Client()
    {
    }

    public Client(Guid id, string name)
    {
        Id = id;
        Name = name;

        RaiseDomainEvent(
            new ClientCreatedDomainEvent(id));
    }
}
```

---

# Domain Events

Permiten comunicar cambios dentro del dominio.

---

## Evento

```csharp
public sealed record ClientCreatedDomainEvent(
    Guid ClientId)
    : DomainEvent;
```

---

## Emitir evento

```csharp
RaiseDomainEvent(
    new ClientCreatedDomainEvent(Id));
```

---

## Obtener eventos

```csharp
var events = aggregate.DomainEvents;
```

---

# AuditableEntity

Agrega auditoría automática.

Campos:

```text
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

---

## Ejemplo

```csharp
public sealed class Client : AuditableEntity<Guid>
{
    public string Name { get; private set; }

    private Client()
    {
    }

    public Client(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

---

# SoftDeletableEntity

Permite borrado lógico.

Campos:

```text
IsDeleted
DeletedAt
DeletedBy
```

---

## Ejemplo

```csharp
public sealed class Client : SoftDeletableEntity<Guid>
{
    public string Name { get; private set; }

    public void Delete(
        string userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }
}
```

---

# ValueObject

Representa objetos inmutables.

No poseen identidad.

Se comparan por valor.

---

## Incorrecto

```csharp
string Email
```

---

## Correcto

```csharp
Email Email
```

---

## Ejemplo

```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

---

## Uso

```csharp
var email1 = new Email("test@test.com");
var email2 = new Email("test@test.com");

Console.WriteLine(email1 == email2);
```

Resultado:

```text
True
```

---

# StronglyTypedId

Evita mezclar IDs.

Problema:

```csharp
Guid clientId;
Guid orderId;

Process(orderId, clientId);
```

Compila.

---

## Solución

```csharp
public sealed record ClientId(Guid Value)
    : StronglyTypedId<Guid>(Value);

public sealed record OrderId(Guid Value)
    : StronglyTypedId<Guid>(Value);
```

---

## Uso

```csharp
ClientId clientId = new(Guid.NewGuid());
OrderId orderId = new(Guid.NewGuid());

Process(clientId, orderId);
```

---

## Error de compilación

```csharp
Process(orderId, clientId);
```

No compila.

---

# DomainException

Errores de dominio.

---

## Ejemplo

```csharp
throw new DomainException(
    "Client already exists");
```

---

# BusinessRuleException

Violación de reglas de negocio.

---

## Ejemplo

```csharp
if (creditLimitExceeded)
{
    throw new BusinessRuleException(
        "Credit limit exceeded");
}
```

---

# IDateTimeProvider

Permite controlar tiempo.

Evita usar:

```csharp
DateTime.UtcNow
```

---

## Uso

```csharp
public sealed class ClientService(
    IDateTimeProvider dateTimeProvider)
{
    public DateTime GetDate()
    {
        return dateTimeProvider.UtcNow;
    }
}
```

---

# ICurrentUser

Permite obtener usuario actual.

---

## Uso

```csharp
public sealed class ClientService(
    ICurrentUser currentUser)
{
    public string? GetUserId()
    {
        return currentUser.UserId;
    }
}
```

---

# ICorrelationContext

Permite rastreo distribuido.

---

## Uso

```csharp
public sealed class ClientService(
    ICorrelationContext correlation)
{
    public string CorrelationId()
    {
        return correlation.CorrelationId;
    }
}
```

---

# Pagination

## Request

```csharp
var request = new PageRequest(
    pageNumber: 1,
    pageSize: 20);
```

---

## Response

```csharp
var result = new PagedResult<ClientDto>(
    items,
    totalCount,
    pageNumber,
    pageSize);
```

---

# Guards

Validaciones rápidas.

---

## NotNull

```csharp
Guard.NotNull(client);
```

---

## NotEmpty

```csharp
Guard.NotEmpty(name);
```

---

## Positive

```csharp
Guard.Positive(quantity);
```

---

# Ejemplo completo

## Aggregate Root

```csharp
public sealed class Client : AuditableEntity<Guid>
{
    public string Name { get; private set; }

    public Email Email { get; private set; }

    private Client()
    {
    }

    public Client(
        Guid id,
        string name,
        Email email)
    {
        Guard.NotEmpty(name);

        Id = id;
        Name = name;
        Email = email;

        RaiseDomainEvent(
            new ClientCreatedDomainEvent(id));
    }

    public Result ChangeEmail(
        Email email)
    {
        if (Email == email)
        {
            return Result.Failure(
                Error.Create(
                    "clients.same_email",
                    "Email is already assigned"));
        }

        Email = email;

        return Result.Success();
    }
}
```

---

# Buenas prácticas

## Sí

```csharp
Email Email
```

```csharp
ClientId ClientId
```

```csharp
Result<T>
```

```csharp
Domain Events
```

```csharp
Aggregate Roots
```

---

## No

```csharp
string Email
```

```csharp
Guid ClientId
```

```csharp
throw new Exception()
```

```csharp
DateTime.UtcNow
```

```csharp
HttpContext dentro del dominio
```

---

# Dependencias

Todos los paquetes del framework dependen de Core:

```text
Validation
Cqrs
Persistence
Postgres
Postgres.EntityFramework
Postgres.Dapper
Mongo
Redis
Messaging
Api
Auth
Observability
Notifications
Reports
Storage
```

Por esta razón Core debe mantenerse extremadamente pequeño, estable y sin dependencias externas.
