# CustomCodeFramework - Uso básico

## Versión actual

```bash
0.1.0-alpha.1
```

---

# Configurar NuGet

## Source local

```bash
dotnet nuget add source /home/maulang/Sistemas/CustomCodeFramework/artifacts/packages \
  --name customcode-local
```

## GitHub Packages

```bash
dotnet nuget add source "https://nuget.pkg.github.com/CustomCodeCR/index.json" \
  --name github \
  --username TU_USUARIO \
  --password TU_GITHUB_TOKEN \
  --store-password-in-clear-text
```

---

# Instalar paquetes

## Core

```bash
dotnet add package CustomCodeFramework.Core --version 0.1.0-alpha.1
```

## CQRS

```bash
dotnet add package CustomCodeFramework.Cqrs --version 0.1.0-alpha.1
```

## Validation

```bash
dotnet add package CustomCodeFramework.Validation --version 0.1.0-alpha.1
```

## Persistence

```bash
dotnet add package CustomCodeFramework.Persistence --version 0.1.0-alpha.1
```

## API

```bash
dotnet add package CustomCodeFramework.Api --version 0.1.0-alpha.1
```

## Auth

```bash
dotnet add package CustomCodeFramework.Auth --version 0.1.0-alpha.1
```

## Observability

```bash
dotnet add package CustomCodeFramework.Observability --version 0.1.0-alpha.1
```

## PostgreSQL

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Postgres.EntityFramework --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Postgres.Dapper --version 0.1.0-alpha.1
```

## MongoDB

```bash
dotnet add package CustomCodeFramework.Mongo --version 0.1.0-alpha.1
```

## Redis

```bash
dotnet add package CustomCodeFramework.Redis --version 0.1.0-alpha.1
```

## Messaging

```bash
dotnet add package CustomCodeFramework.Messaging --version 0.1.0-alpha.1
```

## Reports

```bash
dotnet add package CustomCodeFramework.Reports --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Csv --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Excel --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Word --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Pdf --version 0.1.0-alpha.1
```

## Notifications

```bash
dotnet add package CustomCodeFramework.Notifications --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.Email --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.SignalR --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.Sms --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.WhatsApp --version 0.1.0-alpha.1
```

## Storage

```bash
dotnet add package CustomCodeFramework.Storage --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Storage.Local --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Storage.S3 --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Storage.AzureBlob --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Storage.GoogleCloud --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Storage.Firebase --version 0.1.0-alpha.1
```

---

# Instalación recomendada

## API básica

```bash
dotnet add package CustomCodeFramework.Core --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Validation --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Cqrs --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Persistence --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Api --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Auth --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Observability --version 0.1.0-alpha.1
```

## API con PostgreSQL

```bash
dotnet add package CustomCodeFramework.Postgres --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Postgres.EntityFramework --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Postgres.Dapper --version 0.1.0-alpha.1
```

## API con MongoDB y Redis

```bash
dotnet add package CustomCodeFramework.Mongo --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Redis --version 0.1.0-alpha.1
```

## API con reportes

```bash
dotnet add package CustomCodeFramework.Reports --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Csv --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Excel --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Word --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Reports.Pdf --version 0.1.0-alpha.1
```

## API con notificaciones

```bash
dotnet add package CustomCodeFramework.Notifications --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.Email --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.SignalR --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.Sms --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Notifications.WhatsApp --version 0.1.0-alpha.1
```

## API con storage local

```bash
dotnet add package CustomCodeFramework.Storage --version 0.1.0-alpha.1
dotnet add package CustomCodeFramework.Storage.Local --version 0.1.0-alpha.1
```

---

# Registro básico en Program.cs

```csharp
builder.Services.AddCustomCodeApiWithSwagger(
    title: "My API",
    version: "v1");

builder.Services.AddCustomCodeAuth(
    builder.Configuration,
    addJwt: true,
    addApiKeys: true);

builder.Services.AddCustomCodeCqrs();
builder.Services.AddCustomCodeValidation();
builder.Services.AddCustomCodePersistence();

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "MyService.Api",
    serviceVersion: "0.1.0");
```

---

# Licencia

Copyright © CustomCodeCR

Todos los derechos reservados.
