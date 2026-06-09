# CustomCodeFramework.Validation

## Introducción

`CustomCodeFramework.Validation` centraliza toda la validación de entrada de la aplicación.

Su objetivo es evitar:

- Validaciones manuales repetidas.
- Lógica de validación dentro de endpoints.
- Validaciones dentro de handlers.
- Mensajes inconsistentes.
- Errores de negocio mezclados con errores de validación.

---

# ¿Qué problema resuelve?

Incorrecto:

```csharp
app.MapPost("/clients", async (CreateClientRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Name is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest("Email is required.");
    }

    if (!request.Email.Contains("@"))
    {
        return Results.BadRequest("Invalid email.");
    }

    return Results.Ok();
});
```

Problemas:

```text
Validaciones repetidas
Mensajes inconsistentes
Difícil mantenimiento
No reutilizable
```

---

# Solución

```text
Request
 ↓
Validator
 ↓
ValidationBehavior
 ↓
Handler
```

---

# Responsabilidades

El paquete proporciona:

```text
IValidationService
IValidationRule
ValidationException
ValidationError
ValidationErrorCodes
FluentValidation integration
ValidationResult mapping
Common validation rules
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Validation
```

---

# Registro

```csharp
using CustomCodeFramework.Validation.DependencyInjection;

builder.Services.AddCustomCodeValidation();
```

---

# Dependencias

```text
CustomCodeFramework.Core
FluentValidation
```

---

# Estructura

```text
CustomCodeFramework.Validation
├── Abstractions
│   ├── IValidationService.cs
│   └── IValidationRule.cs
│
├── Errors
│   ├── ValidationError.cs
│   └── ValidationErrorCodes.cs
│
├── Exceptions
│   └── ValidationException.cs
│
├── FluentValidation
│   ├── FluentValidationService.cs
│   ├── FluentValidationExtensions.cs
│   └── CommonRules.cs
│
├── Mappers
│   └── ValidationResultMapper.cs
│
└── DependencyInjection
    └── ServiceCollectionExtensions.cs
```

---

# Flujo de validación

```text
HTTP Request
    ↓
Request DTO
    ↓
FluentValidation Validator
    ↓
ValidationBehavior
    ↓
Command Handler
```

Si la validación falla:

```text
Request
 ↓
Validator
 ↓
ValidationException
 ↓
ProblemDetails
 ↓
400 Bad Request
```

---

# ValidationError

Representa un error de validación.

---

## Ejemplo

```csharp
new ValidationError(
    "Email",
    "Email is required.",
    ValidationErrorCodes.Required);
```

Resultado:

```json
{
  "property": "Email",
  "message": "Email is required.",
  "code": "required"
}
```

---

# ValidationErrorCodes

Catálogo estándar de errores.

---

## Required

```csharp
ValidationErrorCodes.Required
```

Resultado:

```text
required
```

---

## Invalid

```csharp
ValidationErrorCodes.Invalid
```

Resultado:

```text
invalid
```

---

## MaxLength

```csharp
ValidationErrorCodes.MaxLength
```

Resultado:

```text
max_length
```

---

## MinLength

```csharp
ValidationErrorCodes.MinLength
```

Resultado:

```text
min_length
```

---

## GreaterThan

```csharp
ValidationErrorCodes.GreaterThan
```

Resultado:

```text
greater_than
```

---

## LessThan

```csharp
ValidationErrorCodes.LessThan
```

Resultado:

```text
less_than
```

---

# ValidationException

Se lanza cuando una validación falla.

---

## Ejemplo

```csharp
throw new ValidationException(errors);
```

---

## Resultado

```json
{
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

# IValidationService

Servicio principal de validación.

---

## Uso

```csharp
public sealed class ClientService(
    IValidationService validationService)
{
    public async Task ValidateAsync(
        object request,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(
            request,
            cancellationToken);
    }
}
```

---

# Primer Validator

## Request

```csharp
public sealed record CreateClientCommand(
    string Name,
    string Email,
    string PhoneNumber);
```

---

## Validator

```csharp
using FluentValidation;

public sealed class CreateClientCommandValidator
    : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty();
    }
}
```

---

# CommonRules

El framework incorpora reglas reutilizables.

---

## RequiredString

En lugar de:

```csharp
RuleFor(x => x.Name)
    .NotEmpty()
    .WithMessage("Name is required.");
```

Utilizar:

```csharp
RuleFor(x => x.Name)
    .RequiredString();
```

---

## Ejemplo

```csharp
public sealed class CreateClientCommandValidator
    : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .RequiredString();

        RuleFor(x => x.Email)
            .RequiredString()
            .EmailAddress();

        RuleFor(x => x.PhoneNumber)
            .RequiredString();
    }
}
```

---

# FluentValidationExtensions

Permite estandarizar reglas.

---

## Ejemplo

```csharp
RuleFor(x => x.Name)
    .RequiredString()
    .MaximumLength(200);
```

---

# Ejemplo real: Crear Cliente

## Command

```csharp
public sealed record CreateClientCommand(
    string Name,
    string Email,
    string PhoneNumber);
```

---

## Validator

```csharp
using FluentValidation;
using CustomCodeFramework.Validation.FluentValidation;

public sealed class CreateClientCommandValidator
    : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .RequiredString()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .RequiredString()
            .EmailAddress()
            .MaximumLength(300);

        RuleFor(x => x.PhoneNumber)
            .RequiredString()
            .MaximumLength(50);
    }
}
```

---

## Resultado exitoso

```json
{
  "name": "ACME",
  "email": "client@test.com",
  "phoneNumber": "88888888"
}
```

Pasa validación.

---

## Resultado inválido

```json
{
  "name": "",
  "email": "abc",
  "phoneNumber": ""
}
```

---

## Respuesta

```json
{
  "errors": [
    {
      "property": "Name",
      "message": "Name is required.",
      "code": "required"
    },
    {
      "property": "Email",
      "message": "Email is invalid.",
      "code": "invalid"
    },
    {
      "property": "PhoneNumber",
      "message": "Phone number is required.",
      "code": "required"
    }
  ]
}
```

---

# Validación de IDs

---

## Ejemplo

```csharp
public sealed record GetClientQuery(
    Guid ClientId);
```

Validator:

```csharp
using FluentValidation;

public sealed class GetClientQueryValidator
    : AbstractValidator<GetClientQuery>
{
    public GetClientQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();
    }
}
```

---

# Validación de números

---

## Ejemplo

```csharp
public sealed record CreateInvoiceCommand(
    decimal Amount);
```

Validator:

```csharp
using FluentValidation;

public sealed class CreateInvoiceCommandValidator
    : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
```

---

# Validación de fechas

---

## Request

```csharp
public sealed record CreatePromotionCommand(
    DateTime StartDate,
    DateTime EndDate);
```

Validator:

```csharp
using FluentValidation;

public sealed class CreatePromotionCommandValidator
    : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate);
    }
}
```

---

# Validación condicional

---

## Ejemplo

```csharp
public sealed record CreateClientCommand(
    bool IsCompany,
    string? CompanyName);
```

Validator:

```csharp
using FluentValidation;

public sealed class CreateClientCommandValidator
    : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        When(
            x => x.IsCompany,
            () =>
            {
                RuleFor(x => x.CompanyName)
                    .NotEmpty();
            });
    }
}
```

---

# Validación de colecciones

---

## Request

```csharp
public sealed record CreateSalesOrderCommand(
    IReadOnlyCollection<SalesOrderLineRequest> Lines);
```

---

## Validator

```csharp
using FluentValidation;

public sealed class CreateSalesOrderCommandValidator
    : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.Lines)
            .NotEmpty();
    }
}
```

---

# Validación de objetos hijos

---

## Request

```csharp
public sealed record SalesOrderLineRequest(
    Guid ProductId,
    decimal Quantity);
```

---

## Validator

```csharp
using FluentValidation;

public sealed class SalesOrderLineRequestValidator
    : AbstractValidator<SalesOrderLineRequest>
{
    public SalesOrderLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
```

---

## Validator principal

```csharp
using FluentValidation;

public sealed class CreateSalesOrderCommandValidator
    : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleForEach(x => x.Lines)
            .SetValidator(
                new SalesOrderLineRequestValidator());
    }
}
```

---

# Integración con CQRS

El uso principal es mediante `ValidationBehavior`.

---

## Flujo

```text
Endpoint
 ↓
CommandDispatcher
 ↓
ValidationBehavior
 ↓
Validator
 ↓
Handler
```

---

## Ejemplo

```csharp
CreateClientCommand
    ↓
CreateClientCommandValidator
    ↓
ValidationBehavior
    ↓
CreateClientCommandHandler
```

---

# Integración con API

Cuando existe un error:

```csharp
throw new ValidationException(errors);
```

La API devuelve:

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

# Validaciones recomendadas

## Emails

```csharp
RuleFor(x => x.Email)
    .RequiredString()
    .EmailAddress()
    .MaximumLength(300);
```

---

## Nombres

```csharp
RuleFor(x => x.Name)
    .RequiredString()
    .MaximumLength(200);
```

---

## Teléfonos

```csharp
RuleFor(x => x.PhoneNumber)
    .RequiredString()
    .MaximumLength(50);
```

---

## Moneda

```csharp
RuleFor(x => x.Currency)
    .RequiredString()
    .Length(3);
```

---

## Cantidades

```csharp
RuleFor(x => x.Quantity)
    .GreaterThan(0);
```

---

## Porcentajes

```csharp
RuleFor(x => x.DiscountPercentage)
    .InclusiveBetween(0, 100);
```

---

# Organización recomendada

```text
Features
└── Clients
    └── CreateClient
        ├── CreateClientCommand.cs
        ├── CreateClientCommandValidator.cs
        ├── CreateClientCommandHandler.cs
        └── CreateClientEndpoint.cs
```

Cada Command o Query debe tener su propio Validator.

---

# Errores comunes

## Validar dentro del Handler

Incorrecto:

```csharp
if (string.IsNullOrWhiteSpace(command.Name))
{
    throw new Exception();
}
```

Correcto:

```csharp
CreateClientCommandValidator
```

---

## Validar dentro del Endpoint

Incorrecto:

```csharp
if (request.Name == "")
{
    return Results.BadRequest();
}
```

Correcto:

```csharp
Validator
```

---

## Un validator para todo

Incorrecto:

```csharp
ClientValidator
```

Con 500 reglas.

Correcto:

```text
CreateClientCommandValidator
UpdateClientCommandValidator
DeleteClientCommandValidator
```

---

# Flujo completo recomendado

```text
HTTP Request
    ↓
DTO
    ↓
Command
    ↓
ValidationBehavior
    ↓
FluentValidation Validator
    ↓
Handler
    ↓
Repository
    ↓
UnitOfWork
    ↓
Response
```

---

# Resumen

Utilizar `CustomCodeFramework.Validation` para:

- Validar Commands.
- Validar Queries.
- Estandarizar errores.
- Centralizar reglas.
- Integrarse con CQRS.
- Evitar validaciones repetidas.
- Mantener handlers limpios.

La validación debe ejecutarse siempre antes de llegar al handler.
