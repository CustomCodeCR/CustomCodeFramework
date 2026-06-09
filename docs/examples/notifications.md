# CustomCodeFramework.Notifications

## Introducción

`CustomCodeFramework.Notifications` es el paquete base para manejar notificaciones de forma genérica.

Este paquete no debe depender directamente de un proveedor específico como SMTP, SignalR, Twilio, WhatsApp Cloud API u otro proveedor externo.

Su objetivo es definir contratos y modelos comunes para enviar notificaciones por diferentes canales:

```text
Email
SignalR
SMS
WhatsApp
Push notifications en el futuro
```

Los paquetes concretos son:

```text
CustomCodeFramework.Notifications.Email
CustomCodeFramework.Notifications.SignalR
CustomCodeFramework.Notifications.Sms
CustomCodeFramework.Notifications.WhatsApp
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Notifications --version 0.1.0-alpha.1
```

---

# Configuración base

```json
{
  "Notifications": {
    "UseOutbox": false,
    "OutboxBatchSize": 50,
    "OutboxMaxRetryCount": 5,
    "OutboxRetryDelaySeconds": 30
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Notifications.DependencyInjection;

builder.Services.AddCustomCodeNotifications(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Notifications
├── Abstractions
│   ├── INotification.cs
│   ├── INotificationSender.cs
│   ├── INotificationDispatcher.cs
│   ├── INotificationChannel.cs
│   ├── INotificationTemplateRenderer.cs
│   └── INotificationRecipientResolver.cs
│
├── Channels
│   ├── NotificationChannelType.cs
│   ├── NotificationChannelResult.cs
│   └── NotificationChannelFailure.cs
│
├── Messages
│   ├── NotificationMessage.cs
│   ├── NotificationRecipient.cs
│   ├── NotificationAttachment.cs
│   └── NotificationPriority.cs
│
├── Templates
│   ├── NotificationTemplate.cs
│   ├── NotificationTemplateContext.cs
│   └── NotificationTemplateResult.cs
│
├── Delivery
│   ├── NotificationDelivery.cs
│   ├── NotificationDeliveryStatus.cs
│   ├── NotificationDeliveryAttempt.cs
│   └── NotificationDeliveryResult.cs
│
├── Preferences
│   ├── NotificationPreference.cs
│   ├── NotificationPreferenceScope.cs
│   └── NotificationPreferenceResult.cs
│
├── Outbox
│   ├── NotificationOutboxMessage.cs
│   ├── INotificationOutboxStore.cs
│   └── NotificationOutboxProcessor.cs
│
├── Options
│   └── NotificationsOptions.cs
│
└── DependencyInjection
    └── NotificationServiceCollectionExtensions.cs
```

---

# ¿Qué problema resuelve?

Sin un paquete base, cada módulo termina enviando notificaciones de forma diferente:

```csharp
await emailService.SendAsync(...);
await signalRHub.Clients.User(...).SendAsync(...);
await smsClient.SendAsync(...);
```

Problemas:

```text
lógica repetida
proveedores acoplados
difícil cambiar canales
sin tracking de entregas
sin preferencias
sin outbox
sin retries estándar
sin plantillas reutilizables
```

Con `CustomCodeFramework.Notifications`:

```text
Application
    ↓
INotificationSender
    ↓
INotificationDispatcher
    ↓
INotificationChannel
    ↓
Email / SignalR / SMS / WhatsApp
```

---

# NotificationChannelType

Representa el canal por donde se envía la notificación.

Ejemplos:

```text
Email
SignalR
Sms
WhatsApp
```

Uso:

```csharp
using CustomCodeFramework.Notifications.Channels;

var channel = NotificationChannelType.Email;
```

---

# NotificationMessage

Modelo principal de una notificación.

Ejemplo:

```csharp
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

var message = new NotificationMessage
{
    Channel = NotificationChannelType.Email,
    Recipient = new NotificationRecipient
    {
        Email = "client@example.com",
        Name = "Client"
    },
    Subject = "Welcome",
    Body = "Welcome to the system.",
    Priority = NotificationPriority.Normal
};
```

---

# NotificationRecipient

Representa el destinatario.

Puede contener datos para distintos canales:

```csharp
var recipient = new NotificationRecipient
{
    UserId = "user-001",
    Name = "Maurice",
    Email = "maurice@example.com",
    PhoneNumber = "88888888"
};
```

---

# NotificationAttachment

Representa adjuntos.

Ejemplo:

```csharp
var attachment = new NotificationAttachment
{
    FileName = "invoice.pdf",
    ContentType = "application/pdf",
    Content = fileBytes
};
```

Uso:

```csharp
var message = new NotificationMessage
{
    Channel = NotificationChannelType.Email,
    Recipient = recipient,
    Subject = "Invoice",
    Body = "Your invoice is attached.",
    Attachments =
    [
        attachment
    ]
};
```

---

# NotificationPriority

Prioridad de envío.

Ejemplos:

```text
Low
Normal
High
Critical
```

Uso:

```csharp
var message = new NotificationMessage
{
    Priority = NotificationPriority.High
};
```

---

# INotificationSender

Interfaz principal para enviar notificaciones.

Uso:

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class WelcomeNotificationService(
    INotificationSender notificationSender)
{
    public Task SendAsync(
        string email,
        string name,
        CancellationToken cancellationToken)
    {
        return notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Email = email,
                    Name = name
                },
                Subject = "Welcome",
                Body = $"Welcome {name}."
            },
            cancellationToken);
    }
}
```

---

# INotificationDispatcher

Se encarga de resolver el canal correcto y ejecutar el envío.

Flujo:

```text
NotificationMessage
    ↓
INotificationDispatcher
    ↓
buscar canal por NotificationChannelType
    ↓
INotificationChannel.SendAsync
```

Uso conceptual:

```csharp
using CustomCodeFramework.Notifications.Abstractions;

public sealed class NotificationDispatcherExample(
    INotificationDispatcher dispatcher)
{
    public Task DispatchAsync(
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(
            message,
            cancellationToken);
    }
}
```

---

# INotificationChannel

Contrato que implementan los canales concretos.

Ejemplos:

```text
EmailNotificationChannel
SignalRNotificationChannel
SmsNotificationChannel
WhatsAppNotificationChannel
```

Uso conceptual:

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;

public sealed class CustomNotificationChannel : INotificationChannel
{
    public NotificationChannelType ChannelType => NotificationChannelType.Email;

    public Task<NotificationChannelResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(NotificationChannelResult.Success());
    }
}
```

---

# NotificationChannelResult

Resultado del envío por canal.

Ejemplo exitoso:

```csharp
NotificationChannelResult.Success();
```

Ejemplo fallido:

```csharp
NotificationChannelResult.Failure(
    "email.smtp_error",
    "SMTP server rejected the message.");
```

---

# NotificationChannelFailure

Representa error específico de canal.

```csharp
var failure = new NotificationChannelFailure
{
    Code = "email.invalid_recipient",
    Message = "Recipient email is invalid."
};
```

---

# Templates

## INotificationTemplateRenderer

Permite renderizar plantillas.

Ejemplo:

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Templates;

public sealed class WelcomeTemplateService(
    INotificationTemplateRenderer renderer)
{
    public Task<NotificationTemplateResult> RenderAsync(
        string name,
        CancellationToken cancellationToken)
    {
        return renderer.RenderAsync(
            new NotificationTemplate
            {
                TemplateKey = "welcome-email",
                Content = "<h1>Welcome {{name}}</h1>"
            },
            new NotificationTemplateContext
            {
                Data = new Dictionary<string, object?>
                {
                    ["name"] = name
                }
            },
            cancellationToken);
    }
}
```

---

# NotificationTemplate

```csharp
var template = new NotificationTemplate
{
    TemplateKey = "invoice-issued",
    Subject = "Invoice {{invoiceNumber}}",
    Content = "<p>Your invoice total is {{total}}</p>"
};
```

---

# NotificationTemplateContext

```csharp
var context = new NotificationTemplateContext
{
    Data = new Dictionary<string, object?>
    {
        ["invoiceNumber"] = "INV-001",
        ["total"] = 1500m,
        ["currency"] = "CRC"
    }
};
```

---

# NotificationDelivery

Representa una entrega de notificación.

Campos típicos:

```text
DeliveryId
Channel
Recipient
Status
CreatedAtUtc
SentAtUtc
FailedAtUtc
Error
Attempts
```

---

# NotificationDeliveryStatus

Estados típicos:

```text
Pending
Sending
Sent
Failed
Cancelled
```

---

# NotificationDeliveryAttempt

Representa un intento de envío.

Ejemplo:

```csharp
var attempt = new NotificationDeliveryAttempt
{
    AttemptNumber = 1,
    StartedAtUtc = DateTime.UtcNow,
    Status = NotificationDeliveryStatus.Sent
};
```

---

# NotificationDeliveryResult

Resultado general del envío.

```csharp
var result = new NotificationDeliveryResult
{
    IsSuccess = true,
    DeliveryId = Guid.NewGuid(),
    Status = NotificationDeliveryStatus.Sent
};
```

---

# Preferences

Las preferencias permiten saber si un usuario acepta un canal.

Ejemplo:

```csharp
var preference = new NotificationPreference
{
    UserId = "user-001",
    Channel = NotificationChannelType.Email,
    IsEnabled = true,
    Scope = NotificationPreferenceScope.User
};
```

---

# NotificationPreferenceScope

Ejemplos:

```text
Global
Tenant
User
Module
```

---

# INotificationRecipientResolver

Permite resolver destinatarios.

Ejemplo:

```csharp
using CustomCodeFramework.Notifications.Abstractions;

public sealed class ClientRecipientResolver(
    INotificationRecipientResolver recipientResolver)
{
    public Task<NotificationRecipient> ResolveAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return recipientResolver.ResolveAsync(
            userId,
            cancellationToken);
    }
}
```

---

# Notification Outbox

## ¿Para qué sirve?

Evita perder notificaciones cuando ocurre un error temporal.

Flujo:

```text
CommandHandler
    ↓
Crear NotificationOutboxMessage
    ↓
Guardar en base de datos
    ↓
Worker procesa
    ↓
Envia por canal
    ↓
Marca como enviado
```

---

# NotificationOutboxMessage

Ejemplo conceptual:

```csharp
var outboxMessage = new NotificationOutboxMessage
{
    NotificationOutboxMessageId = Guid.NewGuid(),
    Channel = NotificationChannelType.Email,
    Recipient = recipient,
    Subject = "Welcome",
    Body = "Welcome to the system.",
    CreatedAtUtc = DateTime.UtcNow,
    RetryCount = 0
};
```

---

# INotificationOutboxStore

Contrato para guardar y leer notificaciones pendientes.

Uso conceptual:

```csharp
using CustomCodeFramework.Notifications.Outbox;

public sealed class NotificationOutboxExample(
    INotificationOutboxStore outboxStore)
{
    public Task AddAsync(
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        return outboxStore.AddAsync(
            message,
            cancellationToken);
    }
}
```

---

# NotificationOutboxProcessor

Procesa mensajes pendientes.

Uso conceptual:

```csharp
using CustomCodeFramework.Notifications.Outbox;

public sealed class NotificationWorker(
    NotificationOutboxProcessor processor)
{
    public Task ProcessAsync(CancellationToken cancellationToken)
    {
        return processor.ProcessAsync(cancellationToken);
    }
}
```

---

# Ejemplo real: Notificación al crear cliente

## Evento

```csharp
using CustomCodeFramework.Messaging.Events;

[IntegrationEventName("client-created")]
public sealed record ClientCreatedIntegrationEvent(
    Guid ClientId,
    string Name,
    string Email) : IntegrationEvent;
```

---

## Handler

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class SendWelcomeNotificationWhenClientCreatedHandler(
    INotificationSender notificationSender)
    : IIntegrationEventHandler<ClientCreatedIntegrationEvent>
{
    public Task HandleAsync(
        ClientCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Email = integrationEvent.Email,
                    Name = integrationEvent.Name
                },
                Subject = "Welcome",
                Body = $"Welcome {integrationEvent.Name}."
            },
            cancellationToken);
    }
}
```

---

# Ejemplo real: Enviar por varios canales

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class MultiChannelNotificationService(
    INotificationSender notificationSender)
{
    public async Task SendOrderApprovedAsync(
        string userId,
        string email,
        string phoneNumber,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        await notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    UserId = userId,
                    Email = email
                },
                Subject = $"Order {orderNumber} approved",
                Body = $"Your order {orderNumber} was approved."
            },
            cancellationToken);

        await notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.SignalR,
                Recipient = new NotificationRecipient
                {
                    UserId = userId
                },
                Subject = "Order approved",
                Body = $"Your order {orderNumber} was approved."
            },
            cancellationToken);

        await notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.WhatsApp,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = $"Your order {orderNumber} was approved."
            },
            cancellationToken);
    }
}
```

---

# Buenas prácticas Notifications

## Sí

Usar `INotificationSender`.

Usar `NotificationMessage`.

Separar canal base del provider específico.

Usar outbox para notificaciones importantes.

Usar templates para mensajes repetidos.

Respetar preferencias de usuario.

No enviar desde entidades de dominio.

---

## No

No inyectar SMTP directamente en handlers.

No llamar SignalR Hub directamente desde cualquier parte.

No duplicar lógica de notificación por módulo.

No guardar tokens o credenciales en mensajes.

No enviar datos sensibles innecesarios.

---

# CustomCodeFramework.Notifications.Email

## Introducción

`CustomCodeFramework.Notifications.Email` implementa notificaciones por correo electrónico.

Se usa para:

```text
bienvenidas
facturas
reportes
alertas
recuperación de contraseña
notificaciones formales
comunicados
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Notifications.Email --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Notifications": {
    "Email": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UseSsl": false,
      "UseStartTls": true,
      "UserName": "your-email@gmail.com",
      "Password": "your-app-password",
      "FromEmail": "your-email@gmail.com",
      "FromName": "CustomCode",
      "TimeoutSeconds": 30
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Notifications.DependencyInjection;
using CustomCodeFramework.Notifications.Email.DependencyInjection;

builder.Services
    .AddCustomCodeNotifications(builder.Configuration)
    .AddCustomCodeEmailNotifications(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Notifications.Email
├── Abstractions
│   └── IEmailNotificationSender.cs
├── Messages
│   ├── EmailNotificationMessage.cs
│   ├── EmailAddress.cs
│   └── EmailAttachment.cs
├── Templates
│   └── EmailTemplateRenderer.cs
├── Options
│   └── EmailNotificationOptions.cs
└── DependencyInjection
    └── EmailNotificationServiceCollectionExtensions.cs
```

---

# IEmailNotificationSender

Uso directo cuando se necesita enviar específicamente email.

```csharp
using CustomCodeFramework.Notifications.Email.Abstractions;
using CustomCodeFramework.Notifications.Email.Messages;

public sealed class EmailService(
    IEmailNotificationSender emailSender)
{
    public Task SendAsync(CancellationToken cancellationToken)
    {
        return emailSender.SendAsync(
            new EmailNotificationMessage
            {
                To =
                [
                    new EmailAddress
                    {
                        Email = "client@example.com",
                        Name = "Client"
                    }
                ],
                Subject = "Welcome",
                HtmlBody = "<h1>Welcome</h1>",
                TextBody = "Welcome"
            },
            cancellationToken);
    }
}
```

---

# Enviar email usando INotificationSender

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class WelcomeEmailService(
    INotificationSender notificationSender)
{
    public Task SendAsync(
        string email,
        string name,
        CancellationToken cancellationToken)
    {
        return notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Email = email,
                    Name = name
                },
                Subject = "Welcome",
                Body = $"<h1>Welcome {name}</h1>"
            },
            cancellationToken);
    }
}
```

---

# Email con adjunto

```csharp
using CustomCodeFramework.Notifications.Email.Abstractions;
using CustomCodeFramework.Notifications.Email.Messages;

public sealed class InvoiceEmailService(
    IEmailNotificationSender emailSender)
{
    public Task SendInvoiceAsync(
        string email,
        byte[] pdfBytes,
        CancellationToken cancellationToken)
    {
        return emailSender.SendAsync(
            new EmailNotificationMessage
            {
                To =
                [
                    new EmailAddress
                    {
                        Email = email,
                        Name = "Client"
                    }
                ],
                Subject = "Invoice",
                HtmlBody = "<p>Your invoice is attached.</p>",
                Attachments =
                [
                    new EmailAttachment
                    {
                        FileName = "invoice.pdf",
                        ContentType = "application/pdf",
                        Content = pdfBytes
                    }
                ]
            },
            cancellationToken);
    }
}
```

---

# Email con múltiples destinatarios

```csharp
await emailSender.SendAsync(
    new EmailNotificationMessage
    {
        To =
        [
            new EmailAddress
            {
                Email = "client@example.com",
                Name = "Client"
            }
        ],
        Cc =
        [
            new EmailAddress
            {
                Email = "manager@example.com",
                Name = "Manager"
            }
        ],
        Bcc =
        [
            new EmailAddress
            {
                Email = "audit@example.com",
                Name = "Audit"
            }
        ],
        Subject = "Invoice issued",
        HtmlBody = "<p>Invoice was issued.</p>"
    },
    cancellationToken);
```

---

# EmailTemplateRenderer

Uso conceptual:

```csharp
using CustomCodeFramework.Notifications.Email.Templates;

public sealed class EmailTemplateService(
    EmailTemplateRenderer renderer)
{
    public Task<string> RenderWelcomeAsync(string name)
    {
        return renderer.RenderAsync(
            "<h1>Welcome {{name}}</h1>",
            new Dictionary<string, object?>
            {
                ["name"] = name
            });
    }
}
```

---

# Buenas prácticas Email

## Sí

Usar HTML + texto plano cuando sea posible.

Usar app password, no contraseña personal.

Usar templates.

Usar outbox para correos importantes.

No bloquear la respuesta HTTP si el correo puede enviarse luego.

---

## No

No guardar password SMTP en Git.

No enviar archivos enormes por correo.

No enviar tokens sensibles en texto plano sin expiración.

No construir HTML gigante dentro del handler.

---

# CustomCodeFramework.Notifications.SignalR

## Introducción

`CustomCodeFramework.Notifications.SignalR` implementa notificaciones en tiempo real usando SignalR.

Se usa para:

```text
notificaciones en pantalla
actualizaciones de dashboard
alertas internas
mensajes a usuarios conectados
mensajes a grupos
progreso de procesos
estado de reportes
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Notifications.SignalR --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Notifications": {
    "SignalR": {
      "HubPath": "/hubs/notifications",
      "DefaultClientMethod": "ReceiveNotification",
      "UserIdMetadataKey": "userId",
      "GroupMetadataKey": "group",
      "EnableDetailedErrors": false
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Notifications.DependencyInjection;
using CustomCodeFramework.Notifications.SignalR.DependencyInjection;

builder.Services
    .AddCustomCodeNotifications(builder.Configuration)
    .AddCustomCodeSignalRNotifications(builder.Configuration);
```

---

# Mapear hub

```csharp
var app = builder.Build();

app.MapCustomCodeNotificationHub();
```

---

# Estructura

```text
CustomCodeFramework.Notifications.SignalR
├── Abstractions
│   ├── ISignalRNotificationSender.cs
│   └── ISignalRUserResolver.cs
├── Messages
│   ├── SignalRNotificationMessage.cs
│   └── SignalRTarget.cs
├── Hubs
│   └── NotificationHub.cs
├── Options
│   └── SignalRNotificationOptions.cs
└── DependencyInjection
    └── SignalRNotificationServiceCollectionExtensions.cs
```

---

# Enviar a usuario

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class SignalRUserNotificationService(
    INotificationSender sender)
{
    public Task NotifyUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.SignalR,
                Recipient = new NotificationRecipient
                {
                    UserId = userId
                },
                Subject = "Order approved",
                Body = "Your order was approved."
            },
            cancellationToken);
    }
}
```

---

# Enviar a grupo

```csharp
await sender.SendAsync(
    new NotificationMessage
    {
        Channel = NotificationChannelType.SignalR,
        Recipient = new NotificationRecipient(),
        Subject = "Dashboard updated",
        Body = "Dashboard data was refreshed.",
        Data = new Dictionary<string, object?>
        {
            ["group"] = "admins"
        }
    },
    cancellationToken);
```

---

# Enviar evento de progreso

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class ReportProgressNotifier(
    INotificationSender sender)
{
    public Task NotifyProgressAsync(
        string userId,
        Guid reportJobId,
        int progress,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.SignalR,
                Recipient = new NotificationRecipient
                {
                    UserId = userId
                },
                Subject = "Report progress",
                Body = $"Report generation is {progress}% complete.",
                Data = new Dictionary<string, object?>
                {
                    ["reportJobId"] = reportJobId,
                    ["progress"] = progress
                }
            },
            cancellationToken);
    }
}
```

---

# Cliente JavaScript

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/notifications", {
    accessTokenFactory: () => localStorage.getItem("access_token"),
  })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (notification) => {
  console.log("Notification received", notification);
});

await connection.start();
```

---

# ISignalRNotificationSender

Uso directo:

```csharp
using CustomCodeFramework.Notifications.SignalR.Abstractions;
using CustomCodeFramework.Notifications.SignalR.Messages;

public sealed class DirectSignalRService(
    ISignalRNotificationSender sender)
{
    public Task SendAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new SignalRNotificationMessage
            {
                Target = SignalRTarget.User(userId),
                Method = "ReceiveNotification",
                Payload = new
                {
                    title = "Hello",
                    body = "Real time notification"
                }
            },
            cancellationToken);
    }
}
```

---

# SignalRTarget

Ejemplos:

```csharp
SignalRTarget.User("user-001");
SignalRTarget.Group("admins");
SignalRTarget.All();
```

---

# Buenas prácticas SignalR

## Sí

Usar para información en tiempo real.

Usar grupos para roles o salas.

Usar UserId para notificaciones personales.

Mandar payloads pequeños.

Usar reconexión en frontend.

---

## No

No usar SignalR como almacenamiento.

No enviar archivos grandes.

No enviar secretos.

No asumir que el usuario está conectado.

Si el mensaje es crítico, guardar también en base de datos o outbox.

---

# CustomCodeFramework.Notifications.Sms

## Introducción

`CustomCodeFramework.Notifications.Sms` define la base para notificaciones SMS.

Este paquete puede funcionar como abstracción base para proveedores como:

```text
Twilio
AWS SNS
MessageBird
Vonage
Proveedor local
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Notifications.Sms --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Notifications": {
    "Sms": {
      "DefaultCountryCode": "506",
      "ProviderName": "None",
      "Enabled": false
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Notifications.DependencyInjection;
using CustomCodeFramework.Notifications.Sms.DependencyInjection;

builder.Services
    .AddCustomCodeNotifications(builder.Configuration)
    .AddCustomCodeSmsNotifications(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Notifications.Sms
├── Abstractions
│   └── ISmsNotificationSender.cs
├── Messages
│   ├── SmsNotificationMessage.cs
│   └── PhoneNumber.cs
├── Options
│   └── SmsNotificationOptions.cs
└── DependencyInjection
    └── SmsNotificationServiceCollectionExtensions.cs
```

---

# Enviar SMS usando INotificationSender

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class SmsCodeService(
    INotificationSender sender)
{
    public Task SendCodeAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Sms,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = $"Your verification code is {code}."
            },
            cancellationToken);
    }
}
```

---

# ISmsNotificationSender

Uso directo:

```csharp
using CustomCodeFramework.Notifications.Sms.Abstractions;
using CustomCodeFramework.Notifications.Sms.Messages;

public sealed class DirectSmsService(
    ISmsNotificationSender sender)
{
    public Task SendAsync(
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new SmsNotificationMessage
            {
                To = new PhoneNumber
                {
                    CountryCode = "506",
                    Number = phoneNumber
                },
                Body = "Your verification code is 123456."
            },
            cancellationToken);
    }
}
```

---

# PhoneNumber

```csharp
var phone = new PhoneNumber
{
    CountryCode = "506",
    Number = "88888888"
};
```

Resultado lógico:

```text
+50688888888
```

---

# SMS para verificación

```csharp
public sealed class TwoFactorCodeService(
    INotificationSender sender)
{
    public Task SendTwoFactorCodeAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Sms,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = $"Your login code is {code}. It expires in 5 minutes.",
                Priority = NotificationPriority.High
            },
            cancellationToken);
    }
}
```

---

# SMS para alertas

```csharp
public sealed class CriticalAlertSmsService(
    INotificationSender sender)
{
    public Task SendAlertAsync(
        string phoneNumber,
        string alert,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Sms,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = alert,
                Priority = NotificationPriority.Critical
            },
            cancellationToken);
    }
}
```

---

# Buenas prácticas SMS

## Sí

Usar mensajes cortos.

Usar SMS para alertas importantes.

Usar expiración para códigos.

Validar formato de teléfono.

Usar outbox si el mensaje es importante.

---

## No

No mandar información sensible completa.

No mandar mensajes largos.

No usar SMS para contenido HTML.

No depender solo de SMS si es algo crítico de auditoría.

---

# CustomCodeFramework.Notifications.WhatsApp

## Introducción

`CustomCodeFramework.Notifications.WhatsApp` define la base para notificaciones por WhatsApp.

Está pensado para integrarse con proveedores como:

```text
WhatsApp Cloud API
Twilio WhatsApp
360dialog
Proveedor local
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Notifications.WhatsApp --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Notifications": {
    "WhatsApp": {
      "DefaultCountryCode": "506",
      "ProviderName": "None",
      "Enabled": false,
      "DefaultLanguageCode": "es"
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Notifications.DependencyInjection;
using CustomCodeFramework.Notifications.WhatsApp.DependencyInjection;

builder.Services
    .AddCustomCodeNotifications(builder.Configuration)
    .AddCustomCodeWhatsAppNotifications(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Notifications.WhatsApp
├── Abstractions
│   └── IWhatsAppNotificationSender.cs
├── Messages
│   ├── WhatsAppNotificationMessage.cs
│   └── WhatsAppTemplateMessage.cs
├── Options
│   └── WhatsAppNotificationOptions.cs
└── DependencyInjection
    └── WhatsAppNotificationServiceCollectionExtensions.cs
```

---

# Enviar WhatsApp usando INotificationSender

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class WhatsAppOrderService(
    INotificationSender sender)
{
    public Task SendOrderApprovedAsync(
        string phoneNumber,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.WhatsApp,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = $"Your order {orderNumber} was approved."
            },
            cancellationToken);
    }
}
```

---

# IWhatsAppNotificationSender

Uso directo:

```csharp
using CustomCodeFramework.Notifications.WhatsApp.Abstractions;
using CustomCodeFramework.Notifications.WhatsApp.Messages;

public sealed class DirectWhatsAppService(
    IWhatsAppNotificationSender sender)
{
    public Task SendAsync(
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new WhatsAppNotificationMessage
            {
                To = phoneNumber,
                Body = "Your order was approved."
            },
            cancellationToken);
    }
}
```

---

# WhatsAppTemplateMessage

Muchos proveedores requieren templates aprobados.

Ejemplo:

```csharp
using CustomCodeFramework.Notifications.WhatsApp.Messages;

var template = new WhatsAppTemplateMessage
{
    To = "50688888888",
    TemplateName = "order_approved",
    LanguageCode = "es",
    BodyParameters =
    [
        "ORD-001",
        "Maurice"
    ]
};
```

---

# Enviar template usando INotificationSender

```csharp
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class WhatsAppTemplateNotificationService(
    INotificationSender sender)
{
    public Task SendAsync(
        string phoneNumber,
        string orderNumber,
        string clientName,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.WhatsApp,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = string.Empty,
                Data = new Dictionary<string, object?>
                {
                    ["templateName"] = "order_approved",
                    ["languageCode"] = "es",
                    ["bodyParameters"] = new[]
                    {
                        orderNumber,
                        clientName
                    }
                }
            },
            cancellationToken);
    }
}
```

---

# WhatsApp para reportes

```csharp
public sealed class ReportReadyWhatsAppService(
    INotificationSender sender)
{
    public Task NotifyReportReadyAsync(
        string phoneNumber,
        string reportName,
        string downloadUrl,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.WhatsApp,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = $"Your report {reportName} is ready: {downloadUrl}"
            },
            cancellationToken);
    }
}
```

---

# WhatsApp para facturas

```csharp
public sealed class InvoiceWhatsAppService(
    INotificationSender sender)
{
    public Task SendInvoiceIssuedAsync(
        string phoneNumber,
        string invoiceNumber,
        decimal total,
        string currency,
        CancellationToken cancellationToken)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.WhatsApp,
                Recipient = new NotificationRecipient
                {
                    PhoneNumber = phoneNumber
                },
                Body = $"Invoice {invoiceNumber} was issued. Total: {total} {currency}."
            },
            cancellationToken);
    }
}
```

---

# Buenas prácticas WhatsApp

## Sí

Usar templates aprobados para mensajes iniciados por la empresa.

Guardar historial de entrega si es importante.

Validar número con país.

Usar outbox para mensajes críticos.

Usar mensajes cortos y claros.

---

## No

No enviar spam.

No enviar datos sensibles completos.

No depender de WhatsApp como único registro legal.

No guardar tokens del proveedor en Git.

No ignorar políticas del proveedor.

---

# Program.cs completo con todos los canales

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Notifications.DependencyInjection;
using CustomCodeFramework.Notifications.Email.DependencyInjection;
using CustomCodeFramework.Notifications.SignalR.DependencyInjection;
using CustomCodeFramework.Notifications.Sms.DependencyInjection;
using CustomCodeFramework.Notifications.WhatsApp.DependencyInjection;
using CustomCodeFramework.Observability.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Notifications API",
    version: "v1");

builder.Services
    .AddCustomCodeNotifications(builder.Configuration)
    .AddCustomCodeEmailNotifications(builder.Configuration)
    .AddCustomCodeSignalRNotifications(builder.Configuration)
    .AddCustomCodeSmsNotifications(builder.Configuration)
    .AddCustomCodeWhatsAppNotifications(builder.Configuration);

builder.Services.AddCustomCodeObservability(
    builder.Configuration,
    serviceName: "Notifications.Api",
    serviceVersion: "0.1.0");

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapCustomCodeNotificationHub();

app.MapControllers();

app.Run();
```

---

# appsettings.json completo

```json
{
  "Notifications": {
    "UseOutbox": false,
    "OutboxBatchSize": 50,
    "OutboxMaxRetryCount": 5,
    "OutboxRetryDelaySeconds": 30,
    "Email": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UseSsl": false,
      "UseStartTls": true,
      "UserName": "your-email@gmail.com",
      "Password": "your-app-password",
      "FromEmail": "your-email@gmail.com",
      "FromName": "CustomCode",
      "TimeoutSeconds": 30
    },
    "SignalR": {
      "HubPath": "/hubs/notifications",
      "DefaultClientMethod": "ReceiveNotification",
      "UserIdMetadataKey": "userId",
      "GroupMetadataKey": "group",
      "EnableDetailedErrors": false
    },
    "Sms": {
      "DefaultCountryCode": "506",
      "ProviderName": "None",
      "Enabled": false
    },
    "WhatsApp": {
      "DefaultCountryCode": "506",
      "ProviderName": "None",
      "Enabled": false,
      "DefaultLanguageCode": "es"
    }
  }
}
```

---

# Integración con Messaging

Las notificaciones se deben disparar preferiblemente por eventos.

```text
ClientCreatedIntegrationEvent
    ↓
SendWelcomeNotificationHandler
    ↓
INotificationSender
    ↓
Email
```

Ejemplo:

```csharp
using CustomCodeFramework.Messaging.Abstractions;
using CustomCodeFramework.Notifications.Abstractions;
using CustomCodeFramework.Notifications.Channels;
using CustomCodeFramework.Notifications.Messages;

public sealed class SendNotificationWhenInvoiceIssuedHandler(
    INotificationSender sender)
    : IIntegrationEventHandler<InvoiceIssuedIntegrationEvent>
{
    public async Task HandleAsync(
        InvoiceIssuedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        await sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Email = integrationEvent.ClientEmail,
                    Name = integrationEvent.ClientName
                },
                Subject = $"Invoice {integrationEvent.InvoiceNumber}",
                Body = $"Your invoice total is {integrationEvent.Total}."
            },
            cancellationToken);

        await sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.SignalR,
                Recipient = new NotificationRecipient
                {
                    UserId = integrationEvent.UserId
                },
                Subject = "Invoice issued",
                Body = $"Invoice {integrationEvent.InvoiceNumber} was issued."
            },
            cancellationToken);
    }
}
```

---

# Integración con Reports

Cuando un reporte termina:

```text
ReportGeneratedIntegrationEvent
    ↓
SendReportReadyNotificationHandler
    ↓
Email / SignalR / WhatsApp
```

Ejemplo:

```csharp
public sealed class SendReportReadyNotificationHandler(
    INotificationSender sender)
    : IIntegrationEventHandler<ReportGeneratedIntegrationEvent>
{
    public Task HandleAsync(
        ReportGeneratedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.SignalR,
                Recipient = new NotificationRecipient
                {
                    UserId = integrationEvent.RequestedBy
                },
                Subject = "Report ready",
                Body = $"Report {integrationEvent.ReportName} is ready.",
                Data = new Dictionary<string, object?>
                {
                    ["reportId"] = integrationEvent.ReportId,
                    ["downloadUrl"] = integrationEvent.DownloadUrl
                }
            },
            cancellationToken);
    }
}
```

---

# Integración con Storage

Para adjuntar archivos o mandar URLs:

```csharp
public sealed class SendInvoiceWithAttachmentService(
    INotificationSender sender,
    IFileStorage fileStorage)
{
    public async Task SendAsync(
        string email,
        string invoicePath,
        CancellationToken cancellationToken)
    {
        var file = await fileStorage.DownloadAsync(
            new FileDownloadRequest
            {
                Path = invoicePath
            },
            cancellationToken);

        await sender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.Email,
                Recipient = new NotificationRecipient
                {
                    Email = email
                },
                Subject = "Invoice",
                Body = "Your invoice is attached.",
                Attachments =
                [
                    new NotificationAttachment
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        Content = file.Content
                    }
                ]
            },
            cancellationToken);
    }
}
```

---

# Integración con Redis

Usar Redis para evitar envíos duplicados en procesos concurrentes.

```csharp
using CustomCodeFramework.Redis.Abstractions;

public sealed class NotificationDeduplicationService(
    IDistributedLock distributedLock,
    INotificationSender sender)
{
    public async Task SendOnceAsync(
        string notificationKey,
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        await using var handle = await distributedLock.AcquireAsync(
            $"locks:notifications:{notificationKey}",
            TimeSpan.FromMinutes(1),
            cancellationToken);

        if (handle is null)
        {
            return;
        }

        await sender.SendAsync(
            message,
            cancellationToken);
    }
}
```

---

# Organización recomendada

```text
Infrastructure
└── Notifications
    ├── Email
    │   ├── EmailTemplates
    │   └── EmailNotificationServices.cs
    │
    ├── SignalR
    │   └── SignalRNotificationServices.cs
    │
    ├── Sms
    │   └── SmsNotificationServices.cs
    │
    ├── WhatsApp
    │   └── WhatsAppNotificationServices.cs
    │
    └── Handlers
        ├── SendWelcomeNotificationHandler.cs
        ├── SendInvoiceIssuedNotificationHandler.cs
        └── SendReportReadyNotificationHandler.cs
```

---

# Errores comunes

## Enviar desde el dominio

Incorrecto:

```csharp
public sealed class Client
{
    public void Created()
    {
        emailSender.Send(...);
    }
}
```

Correcto:

```text
Domain Event
    ↓
Integration Event
    ↓
Notification Handler
```

---

## Acoplarse a SMTP en Application

Incorrecto:

```csharp
public sealed class CreateClientCommandHandler(SmtpClient smtp)
{
}
```

Correcto:

```csharp
public sealed class SendWelcomeNotificationHandler(
    INotificationSender sender)
{
}
```

---

## Asumir que SignalR siempre entrega

SignalR solo entrega si el usuario está conectado.

Para notificaciones críticas:

```text
guardar en base de datos
enviar email
usar outbox
mostrar historial
```

---

## No usar outbox para mensajes importantes

Incorrecto:

```csharp
await sender.SendAsync(message);
```

sin persistencia para mensajes críticos.

Correcto:

```text
NotificationOutboxMessage
    ↓
NotificationOutboxProcessor
```

---

# Resumen

## CustomCodeFramework.Notifications

Usar para:

```text
contratos base
mensajes genéricos
dispatcher
canales
templates
delivery tracking
preferences
outbox
```

## CustomCodeFramework.Notifications.Email

Usar para:

```text
correo electrónico
HTML email
adjuntos
facturas
reportes
mensajes formales
```

## CustomCodeFramework.Notifications.SignalR

Usar para:

```text
tiempo real
notificaciones en pantalla
dashboards
progreso
usuarios conectados
grupos
```

## CustomCodeFramework.Notifications.Sms

Usar para:

```text
códigos
alertas cortas
2FA
mensajes críticos simples
```

## CustomCodeFramework.Notifications.WhatsApp

Usar para:

```text
avisos al cliente
templates aprobados
seguimiento de órdenes
facturas
reportes listos
```

Combinación recomendada:

```text
Messaging -> dispara eventos
Notifications -> decide canal
Email/SignalR/SMS/WhatsApp -> envían
Outbox -> garantiza reintentos
Redis -> evita duplicados
Storage -> adjuntos o URLs
Observability -> logs y métricas
```
