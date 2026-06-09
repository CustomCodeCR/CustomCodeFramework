# CustomCodeFramework.Storage

## Introducción

`CustomCodeFramework.Storage` es el paquete base para manejar almacenamiento de archivos de forma genérica.

Este paquete no debe depender directamente de un proveedor específico.

Su objetivo es definir contratos y modelos comunes para trabajar con diferentes proveedores:

```text
Local Storage
AWS S3
Azure Blob Storage
Google Cloud Storage
Firebase Storage
```

Los paquetes concretos son:

```text
CustomCodeFramework.Storage.Local
CustomCodeFramework.Storage.S3
CustomCodeFramework.Storage.AzureBlob
CustomCodeFramework.Storage.GoogleCloud
CustomCodeFramework.Storage.Firebase
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Storage --version 0.1.0-alpha.1
```

---

# Configuración base

```json
{
  "Storage": {
    "ProviderName": "Local",
    "DefaultBucket": null,
    "DefaultFolder": "files",
    "PublicBaseUrl": "https://localhost:5001/storage",
    "MaxFileSizeInBytes": 26214400,
    "ValidateFileSignature": false
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Storage.DependencyInjection;

builder.Services.AddCustomCodeStorage(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Storage
├── Abstractions
│   ├── IFileStorage.cs
│   ├── IFileStorageProvider.cs
│   ├── IFileNameGenerator.cs
│   ├── IFilePathBuilder.cs
│   ├── IContentTypeProvider.cs
│   └── IStorageUrlGenerator.cs
│
├── Files
│   ├── StoredFile.cs
│   ├── FileObject.cs
│   ├── FileMetadata.cs
│   ├── FileContent.cs
│   └── FileChecksum.cs
│
├── Uploads
│   ├── FileUploadRequest.cs
│   ├── FileUploadResult.cs
│   ├── FileUploadOptions.cs
│   └── FileUploadValidationResult.cs
│
├── Downloads
│   ├── FileDownloadRequest.cs
│   ├── FileDownloadResult.cs
│   └── FileDownloadOptions.cs
│
├── Paths
│   ├── StoragePath.cs
│   ├── StorageBucket.cs
│   ├── StorageFolder.cs
│   └── StoragePathBuilder.cs
│
├── ContentTypes
│   ├── ContentType.cs
│   ├── ContentTypeMap.cs
│   └── ContentTypeValidator.cs
│
├── Validation
│   ├── FileSizeValidator.cs
│   ├── FileExtensionValidator.cs
│   ├── FileSignatureValidator.cs
│   └── FileValidationOptions.cs
│
├── Security
│   ├── FileAccessPolicy.cs
│   ├── FileVisibility.cs
│   ├── FilePermission.cs
│   └── SignedUrlOptions.cs
│
├── Options
│   └── StorageOptions.cs
│
└── DependencyInjection
    └── StorageServiceCollectionExtensions.cs
```

---

# ¿Qué problema resuelve?

Sin una abstracción común, cada módulo termina guardando archivos diferente:

```csharp
File.WriteAllBytes(path, bytes);
await s3Client.PutObjectAsync(...);
await blobClient.UploadAsync(...);
```

Problemas:

```text
código acoplado al proveedor
difícil cambiar de local a S3
validación duplicada
paths inconsistentes
nombres de archivos inseguros
sin URLs firmadas estándar
sin content types estándar
sin control de visibilidad
```

Con `CustomCodeFramework.Storage`:

```text
Application
    ↓
IFileStorage
    ↓
IFileStorageProvider
    ↓
Local / S3 / Azure Blob / Google Cloud / Firebase
```

---

# IFileStorage

Interfaz principal para trabajar con archivos.

Uso típico:

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Uploads;
using CustomCodeFramework.Storage.Security;

public sealed class ClientDocumentService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string uploadedBy,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = fileName,
                Content = stream,
                ContentType = contentType,
                SizeInBytes = stream.Length,
                UploadedBy = uploadedBy,
                Options = new FileUploadOptions
                {
                    Folder = "clients/documents",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".pdf", ".jpg", ".png"],
                    AllowedContentTypes =
                    [
                        "application/pdf",
                        "image/jpeg",
                        "image/png"
                    ]
                }
            },
            cancellationToken);
    }
}
```

---

# IFileStorageProvider

Interfaz que implementan los proveedores concretos.

Ejemplos:

```text
LocalFileStorage
S3FileStorage
AzureBlobFileStorage
GoogleCloudFileStorage
FirebaseFileStorage
```

Uso conceptual:

```csharp
using CustomCodeFramework.Storage.Abstractions;

public sealed class StorageProviderService(
    IFileStorageProvider provider)
{
    public Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return provider.ExistsAsync(
            path,
            cancellationToken);
    }
}
```

---

# FileUploadRequest

Modelo para subir archivos.

```csharp
using CustomCodeFramework.Storage.Uploads;
using CustomCodeFramework.Storage.Security;

var request = new FileUploadRequest
{
    FileName = "invoice.pdf",
    Content = stream,
    ContentType = "application/pdf",
    SizeInBytes = stream.Length,
    UploadedBy = "user-001",
    Options = new FileUploadOptions
    {
        Folder = "invoices",
        Visibility = FileVisibility.Private,
        AllowedExtensions = [".pdf"],
        AllowedContentTypes = ["application/pdf"]
    }
};
```

---

# FileUploadResult

Resultado de una subida.

```csharp
var result = new FileUploadResult
{
    FileId = Guid.NewGuid(),
    FileName = "invoice.pdf",
    Path = "invoices/2026/06/invoice.pdf",
    ContentType = "application/pdf",
    SizeInBytes = 20480,
    Url = "https://cdn.example.com/invoices/2026/06/invoice.pdf"
};
```

---

# FileUploadOptions

Opciones de subida.

```csharp
var options = new FileUploadOptions
{
    Folder = "clients/documents",
    Visibility = FileVisibility.Private,
    AllowedExtensions = [".pdf", ".jpg", ".png"],
    AllowedContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png"
    ],
    MaxFileSizeInBytes = 10 * 1024 * 1024,
    OverwriteIfExists = false
};
```

---

# FileDownloadRequest

Modelo para descargar archivos.

```csharp
using CustomCodeFramework.Storage.Downloads;

var request = new FileDownloadRequest
{
    Path = "invoices/2026/06/invoice.pdf"
};
```

---

# FileDownloadResult

Resultado de descarga.

```csharp
var result = new FileDownloadResult
{
    FileName = "invoice.pdf",
    ContentType = "application/pdf",
    Content = fileBytes,
    SizeInBytes = fileBytes.Length
};
```

---

# Upload básico

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Uploads;
using CustomCodeFramework.Storage.Security;

public sealed class FileUploadService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = "document.pdf",
                Content = content,
                ContentType = "application/pdf",
                SizeInBytes = content.Length,
                UploadedBy = "system",
                Options = new FileUploadOptions
                {
                    Folder = "documents",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".pdf"],
                    AllowedContentTypes = ["application/pdf"]
                }
            },
            cancellationToken);
    }
}
```

---

# Download básico

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Downloads;

public sealed class FileDownloadService(
    IFileStorage fileStorage)
{
    public Task<FileDownloadResult> DownloadAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return fileStorage.DownloadAsync(
            new FileDownloadRequest
            {
                Path = path
            },
            cancellationToken);
    }
}
```

---

# Delete básico

```csharp
using CustomCodeFramework.Storage.Abstractions;

public sealed class FileDeleteService(
    IFileStorage fileStorage)
{
    public Task DeleteAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return fileStorage.DeleteAsync(
            path,
            cancellationToken);
    }
}
```

---

# FileVisibility

Define visibilidad del archivo.

Valores típicos:

```text
Private
Public
Internal
```

Uso:

```csharp
Visibility = FileVisibility.Private
```

---

# FilePermission

Permisos para URLs firmadas.

```text
Read
Write
Delete
```

Uso:

```csharp
Permission = FilePermission.Read
```

---

# SignedUrlOptions

Opciones para URL firmada.

```csharp
using CustomCodeFramework.Storage.Security;

var options = new SignedUrlOptions
{
    Permission = FilePermission.Read,
    ExpiresIn = TimeSpan.FromMinutes(15)
};
```

---

# Generar URL firmada

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;

public sealed class SignedUrlService(
    IFileStorageProvider provider)
{
    public Task<string?> CreateReadUrlAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return provider.GenerateSignedUrlAsync(
            path,
            new SignedUrlOptions
            {
                Permission = FilePermission.Read,
                ExpiresIn = TimeSpan.FromMinutes(15)
            },
            cancellationToken);
    }
}
```

---

# IFileNameGenerator

Genera nombres seguros.

Uso conceptual:

```csharp
using CustomCodeFramework.Storage.Abstractions;

public sealed class FileNameService(
    IFileNameGenerator fileNameGenerator)
{
    public string Generate(string originalFileName)
    {
        return fileNameGenerator.Generate(
            originalFileName);
    }
}
```

Ejemplo resultado:

```text
invoice_20260609_8f5b4c2a.pdf
```

---

# IFilePathBuilder

Construye rutas consistentes.

```csharp
using CustomCodeFramework.Storage.Abstractions;

public sealed class FilePathService(
    IFilePathBuilder pathBuilder)
{
    public string Build(Guid clientId, string fileName)
    {
        return pathBuilder.Build(
            "clients",
            clientId.ToString(),
            fileName);
    }
}
```

Resultado:

```text
clients/{clientId}/invoice.pdf
```

---

# IContentTypeProvider

Detecta content type.

```csharp
using CustomCodeFramework.Storage.Abstractions;

public sealed class ContentTypeService(
    IContentTypeProvider contentTypeProvider)
{
    public string GetContentType(string fileName)
    {
        return contentTypeProvider.GetContentType(fileName);
    }
}
```

Resultado:

```text
invoice.pdf -> application/pdf
image.png -> image/png
```

---

# FileMetadata

Metadata del archivo.

```csharp
var metadata = new FileMetadata
{
    UploadedBy = "user-001",
    UploadedAtUtc = DateTime.UtcNow,
    Tags = new Dictionary<string, string>
    {
        ["module"] = "invoices",
        ["invoiceId"] = invoiceId.ToString()
    }
};
```

---

# FileChecksum

Representa hash del archivo.

```csharp
var checksum = new FileChecksum
{
    Algorithm = "SHA256",
    Value = "abc123..."
};
```

---

# Validaciones

## FileSizeValidator

Valida tamaño máximo.

```csharp
MaxFileSizeInBytes = 10 * 1024 * 1024
```

---

## FileExtensionValidator

Valida extensiones.

```csharp
AllowedExtensions = [".pdf", ".jpg", ".png"]
```

---

## ContentTypeValidator

Valida content type.

```csharp
AllowedContentTypes =
[
    "application/pdf",
    "image/jpeg",
    "image/png"
]
```

---

## FileSignatureValidator

Valida firma real del archivo.

Ejemplo:

```text
PDF inicia con %PDF
PNG inicia con bytes 89 50 4E 47
JPG inicia con FF D8
```

Configuración:

```json
{
  "Storage": {
    "ValidateFileSignature": true
  }
}
```

---

# Ejemplo real: Subir documento de cliente

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class ClientDocumentService(
    IFileStorage fileStorage)
{
    public async Task<ClientDocumentResult> UploadDocumentAsync(
        Guid clientId,
        Stream file,
        string fileName,
        string contentType,
        string uploadedBy,
        CancellationToken cancellationToken)
    {
        var result = await fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = fileName,
                Content = file,
                ContentType = contentType,
                SizeInBytes = file.Length,
                UploadedBy = uploadedBy,
                Options = new FileUploadOptions
                {
                    Folder = $"clients/{clientId}/documents",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".pdf", ".jpg", ".png"],
                    AllowedContentTypes =
                    [
                        "application/pdf",
                        "image/jpeg",
                        "image/png"
                    ],
                    MaxFileSizeInBytes = 10 * 1024 * 1024
                }
            },
            cancellationToken);

        return new ClientDocumentResult(
            result.FileId,
            result.FileName,
            result.Path,
            result.ContentType,
            result.SizeInBytes);
    }
}

public sealed record ClientDocumentResult(
    Guid FileId,
    string FileName,
    string Path,
    string ContentType,
    long SizeInBytes);
```

---

# Ejemplo real: Descargar factura

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Downloads;

public sealed class InvoiceDownloadService(
    IFileStorage fileStorage)
{
    public Task<FileDownloadResult> DownloadInvoiceAsync(
        string invoicePath,
        CancellationToken cancellationToken)
    {
        return fileStorage.DownloadAsync(
            new FileDownloadRequest
            {
                Path = invoicePath
            },
            cancellationToken);
    }
}
```

Endpoint:

```csharp
app.MapGet("/api/v1/files/download", async (
    string path,
    IFileStorage fileStorage,
    CancellationToken cancellationToken) =>
{
    var result = await fileStorage.DownloadAsync(
        new FileDownloadRequest
        {
            Path = path
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});
```

---

# Ejemplo real: Guardar reporte generado

```csharp
using CustomCodeFramework.Reports.Results;
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class ReportFileStorageService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> StoreReportAsync(
        ReportGenerationResult report,
        string userId,
        CancellationToken cancellationToken)
    {
        var stream = new MemoryStream(report.Content);

        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = report.FileName,
                Content = stream,
                ContentType = report.ContentType,
                SizeInBytes = report.Content.Length,
                UploadedBy = userId,
                Options = new FileUploadOptions
                {
                    Folder = "reports",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".pdf", ".xlsx", ".csv", ".docx"]
                }
            },
            cancellationToken);
    }
}
```

---

# CustomCodeFramework.Storage.Local

## Introducción

`CustomCodeFramework.Storage.Local` implementa almacenamiento en disco local.

Se recomienda para:

```text
desarrollo local
ambientes simples
pruebas
archivos temporales
servidores internos pequeños
```

No se recomienda como opción principal para sistemas distribuidos con varias instancias, a menos que se use un volumen compartido.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Storage.Local --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Storage": {
    "ProviderName": "Local",
    "DefaultFolder": "files",
    "MaxFileSizeInBytes": 26214400,
    "ValidateFileSignature": false,
    "Local": {
      "RootPath": "storage",
      "CreateDirectoryIfNotExists": true,
      "UsePublicUrl": true,
      "PublicBaseUrl": "https://localhost:5001/storage"
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.Local.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeLocalStorage(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Storage.Local
├── Local
│   ├── LocalFileStorage.cs
│   ├── LocalStorageOptions.cs
│   └── LocalStoragePathResolver.cs
└── DependencyInjection
    └── LocalStorageServiceCollectionExtensions.cs
```

---

# Exponer archivos públicos

```csharp
using Microsoft.Extensions.FileProviders;

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.GetFullPath("storage")),
    RequestPath = "/storage"
});
```

---

# Upload local

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class LocalUploadExample(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = "test.pdf",
                Content = stream,
                ContentType = "application/pdf",
                SizeInBytes = stream.Length,
                UploadedBy = "system",
                Options = new FileUploadOptions
                {
                    Folder = "test",
                    Visibility = FileVisibility.Public,
                    AllowedExtensions = [".pdf"]
                }
            },
            cancellationToken);
    }
}
```

---

# Ruta esperada

```text
storage/test/test.pdf
```

URL esperada si es público:

```text
https://localhost:5001/storage/test/test.pdf
```

---

# Docker con volumen local

```yaml
services:
  api:
    image: my-api
    volumes:
      - ./storage:/app/storage
    ports:
      - "5001:8080"
```

---

# Buenas prácticas Local

## Sí

Usar para desarrollo.

Usar volumen persistente en Docker.

Separar archivos públicos y privados.

Validar extensiones y content type.

---

## No

No usar disco local con múltiples instancias sin volumen compartido.

No guardar archivos privados dentro de wwwroot sin control.

No confiar solo en extensión del archivo.

---

# CustomCodeFramework.Storage.S3

## Introducción

`CustomCodeFramework.Storage.S3` implementa almacenamiento compatible con AWS S3.

También puede usarse con proveedores compatibles como:

```text
MinIO
Wasabi
DigitalOcean Spaces
Cloudflare R2
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Storage.S3 --version 0.1.0-alpha.1
```

---

# Configuración AWS S3

```json
{
  "Storage": {
    "ProviderName": "S3",
    "DefaultFolder": "files",
    "S3": {
      "BucketName": "customcode-files",
      "Region": "us-east-1",
      "AccessKey": "CHANGE_ME",
      "SecretKey": "CHANGE_ME",
      "ServiceUrl": null,
      "ForcePathStyle": false,
      "UsePublicUrl": true,
      "PublicBaseUrl": "https://cdn.example.com",
      "SignedUrlExpirationMinutes": 15
    }
  }
}
```

---

# Configuración MinIO

```json
{
  "Storage": {
    "ProviderName": "S3",
    "DefaultFolder": "files",
    "S3": {
      "BucketName": "customcode",
      "Region": "us-east-1",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin",
      "ServiceUrl": "http://localhost:9000",
      "ForcePathStyle": true,
      "UsePublicUrl": true,
      "PublicBaseUrl": "http://localhost:9000/customcode",
      "SignedUrlExpirationMinutes": 15
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.S3.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeS3Storage(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Storage.S3
├── S3
│   ├── S3FileStorage.cs
│   ├── S3StorageOptions.cs
│   ├── S3StoragePathResolver.cs
│   └── S3SignedUrlGenerator.cs
└── DependencyInjection
    └── S3StorageServiceCollectionExtensions.cs
```

---

# Upload a S3

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class S3UploadService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = fileName,
                Content = stream,
                ContentType = "application/pdf",
                SizeInBytes = stream.Length,
                UploadedBy = "system",
                Options = new FileUploadOptions
                {
                    Folder = "invoices",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".pdf"]
                }
            },
            cancellationToken);
    }
}
```

---

# URL firmada S3

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;

public sealed class S3SignedUrlService(
    IFileStorageProvider provider)
{
    public Task<string?> GenerateAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return provider.GenerateSignedUrlAsync(
            path,
            new SignedUrlOptions
            {
                Permission = FilePermission.Read,
                ExpiresIn = TimeSpan.FromMinutes(15)
            },
            cancellationToken);
    }
}
```

---

# Docker Compose MinIO

```yaml
services:
  minio:
    image: minio/minio
    container_name: customcode-minio
    command: server /data --console-address ":9001"
    ports:
      - "9000:9000"
      - "9001:9001"
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_PASSWORD: minioadmin
    volumes:
      - minio_data:/data

volumes:
  minio_data:
```

---

# Buenas prácticas S3

## Sí

Usar bucket privado.

Usar signed URLs.

Usar CDN para archivos públicos.

Rotar access keys.

Usar IAM con permisos mínimos.

---

## No

No guardar access key y secret en Git.

No hacer público todo el bucket.

No usar URLs públicas para archivos privados.

---

# CustomCodeFramework.Storage.AzureBlob

## Introducción

`CustomCodeFramework.Storage.AzureBlob` implementa almacenamiento con Azure Blob Storage.

Se recomienda para sistemas desplegados en Azure.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Storage.AzureBlob --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Storage": {
    "ProviderName": "AzureBlob",
    "DefaultFolder": "files",
    "AzureBlob": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=CHANGE_ME;AccountKey=CHANGE_ME;EndpointSuffix=core.windows.net",
      "ContainerName": "customcode",
      "CreateContainerIfNotExists": true,
      "UsePublicUrl": false,
      "PublicBaseUrl": null,
      "SignedUrlExpirationMinutes": 15
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Storage.AzureBlob.DependencyInjection;
using CustomCodeFramework.Storage.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeAzureBlobStorage(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Storage.AzureBlob
├── AzureBlob
│   ├── AzureBlobFileStorage.cs
│   ├── AzureBlobStorageOptions.cs
│   ├── AzureBlobPathResolver.cs
│   └── AzureBlobSignedUrlGenerator.cs
└── DependencyInjection
    └── AzureBlobStorageServiceCollectionExtensions.cs
```

---

# Upload a Azure Blob

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class AzureBlobUploadService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = "contract.pdf",
                Content = stream,
                ContentType = "application/pdf",
                SizeInBytes = stream.Length,
                UploadedBy = "system",
                Options = new FileUploadOptions
                {
                    Folder = "contracts",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".pdf"]
                }
            },
            cancellationToken);
    }
}
```

---

# URL firmada Azure Blob

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;

public sealed class AzureBlobSignedUrlService(
    IFileStorageProvider provider)
{
    public Task<string?> GenerateAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return provider.GenerateSignedUrlAsync(
            path,
            new SignedUrlOptions
            {
                Permission = FilePermission.Read,
                ExpiresIn = TimeSpan.FromMinutes(15)
            },
            cancellationToken);
    }
}
```

---

# Buenas prácticas Azure Blob

## Sí

Usar contenedores privados.

Usar SAS URLs para acceso temporal.

Usar Managed Identity cuando sea posible.

Usar lifecycle policies para limpiar archivos temporales.

---

## No

No exponer connection string en Git.

No usar contenedores públicos para documentos privados.

No usar SAS sin expiración.

---

# CustomCodeFramework.Storage.GoogleCloud

## Introducción

`CustomCodeFramework.Storage.GoogleCloud` implementa almacenamiento con Google Cloud Storage.

Se recomienda para sistemas desplegados en Google Cloud Platform.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Storage.GoogleCloud --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Storage": {
    "ProviderName": "GoogleCloud",
    "DefaultFolder": "files",
    "GoogleCloud": {
      "BucketName": "customcode-files",
      "ProjectId": "my-gcp-project",
      "CredentialFilePath": "/secrets/gcp-service-account.json",
      "CreateBucketIfNotExists": false,
      "UsePublicUrl": false,
      "PublicBaseUrl": null,
      "SignedUrlExpirationMinutes": 15
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.GoogleCloud.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeGoogleCloudStorage(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Storage.GoogleCloud
├── GoogleCloud
│   ├── GoogleCloudFileStorage.cs
│   ├── GoogleCloudStorageOptions.cs
│   ├── GoogleCloudPathResolver.cs
│   └── GoogleCloudSignedUrlGenerator.cs
└── DependencyInjection
    └── GoogleCloudStorageServiceCollectionExtensions.cs
```

---

# Upload a Google Cloud Storage

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class GoogleCloudUploadService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = "report.xlsx",
                Content = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                SizeInBytes = stream.Length,
                UploadedBy = "system",
                Options = new FileUploadOptions
                {
                    Folder = "reports",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".xlsx"]
                }
            },
            cancellationToken);
    }
}
```

---

# URL firmada Google Cloud

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;

public sealed class GoogleCloudSignedUrlService(
    IFileStorageProvider provider)
{
    public Task<string?> GenerateAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return provider.GenerateSignedUrlAsync(
            path,
            new SignedUrlOptions
            {
                Permission = FilePermission.Read,
                ExpiresIn = TimeSpan.FromMinutes(15)
            },
            cancellationToken);
    }
}
```

---

# Buenas prácticas Google Cloud

## Sí

Usar service accounts con permisos mínimos.

Usar buckets privados.

Usar signed URLs.

Usar lifecycle rules.

No guardar credenciales en Git.

---

# CustomCodeFramework.Storage.Firebase

## Introducción

`CustomCodeFramework.Storage.Firebase` implementa almacenamiento usando Firebase Storage.

Se recomienda para aplicaciones que ya usan Firebase o apps móviles/web donde Firebase forma parte del ecosistema.

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Storage.Firebase --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Storage": {
    "ProviderName": "Firebase",
    "DefaultFolder": "files",
    "Firebase": {
      "BucketName": "my-project.appspot.com",
      "ProjectId": "my-project",
      "CredentialFilePath": "/secrets/firebase-service-account.json",
      "UsePublicUrl": true,
      "PublicBaseUrl": null,
      "SignedUrlExpirationMinutes": 15,
      "UseFirebaseDownloadToken": true
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.Firebase.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeFirebaseStorage(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Storage.Firebase
├── Firebase
│   ├── FirebaseFileStorage.cs
│   ├── FirebaseStorageOptions.cs
│   ├── FirebasePathResolver.cs
│   └── FirebaseSignedUrlGenerator.cs
└── DependencyInjection
    └── FirebaseStorageServiceCollectionExtensions.cs
```

---

# Upload a Firebase Storage

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

public sealed class FirebaseUploadService(
    IFileStorage fileStorage)
{
    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        return fileStorage.UploadAsync(
            new FileUploadRequest
            {
                FileName = "profile-image.png",
                Content = stream,
                ContentType = "image/png",
                SizeInBytes = stream.Length,
                UploadedBy = "system",
                Options = new FileUploadOptions
                {
                    Folder = "users/profile-images",
                    Visibility = FileVisibility.Private,
                    AllowedExtensions = [".png"],
                    AllowedContentTypes = ["image/png"]
                }
            },
            cancellationToken);
    }
}
```

---

# URL firmada Firebase

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;

public sealed class FirebaseSignedUrlService(
    IFileStorageProvider provider)
{
    public Task<string?> GenerateAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return provider.GenerateSignedUrlAsync(
            path,
            new SignedUrlOptions
            {
                Permission = FilePermission.Read,
                ExpiresIn = TimeSpan.FromMinutes(15)
            },
            cancellationToken);
    }
}
```

---

# Buenas prácticas Firebase

## Sí

Usar reglas de seguridad.

Usar service account.

Usar tokens temporales.

Separar archivos públicos y privados.

---

## No

No dejar reglas abiertas.

No guardar service account en Git.

No usar Firebase Storage para datos que deberían estar en base de datos.

---

# Program.cs con todos los providers

Normalmente solo se registra un provider concreto por servicio.

Ejemplo con Local:

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.Local.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Storage API",
    version: "v1");

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeLocalStorage(builder.Configuration);

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapControllers();

app.Run();
```

Ejemplo con S3:

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.S3.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeS3Storage(builder.Configuration);
```

Ejemplo con Azure Blob:

```csharp
using CustomCodeFramework.Storage.AzureBlob.DependencyInjection;
using CustomCodeFramework.Storage.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeAzureBlobStorage(builder.Configuration);
```

Ejemplo con Google Cloud:

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.GoogleCloud.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeGoogleCloudStorage(builder.Configuration);
```

Ejemplo con Firebase:

```csharp
using CustomCodeFramework.Storage.DependencyInjection;
using CustomCodeFramework.Storage.Firebase.DependencyInjection;

builder.Services
    .AddCustomCodeStorage(builder.Configuration)
    .AddCustomCodeFirebaseStorage(builder.Configuration);
```

---

# Endpoint completo de upload

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;
using CustomCodeFramework.Storage.Uploads;

app.MapPost("/api/v1/files", async (
    IFormFile file,
    IFileStorage fileStorage,
    CancellationToken cancellationToken) =>
{
    await using var stream = file.OpenReadStream();

    var result = await fileStorage.UploadAsync(
        new FileUploadRequest
        {
            FileName = file.FileName,
            Content = stream,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            UploadedBy = "system",
            Options = new FileUploadOptions
            {
                Folder = "uploads",
                Visibility = FileVisibility.Private,
                AllowedExtensions = [".pdf", ".jpg", ".png", ".xlsx"],
                AllowedContentTypes =
                [
                    "application/pdf",
                    "image/jpeg",
                    "image/png",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                ],
                MaxFileSizeInBytes = 25 * 1024 * 1024
            }
        },
        cancellationToken);

    return Results.Ok(result);
})
.DisableAntiforgery();
```

---

# Endpoint completo de download

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Downloads;

app.MapGet("/api/v1/files/download", async (
    string path,
    IFileStorage fileStorage,
    CancellationToken cancellationToken) =>
{
    var result = await fileStorage.DownloadAsync(
        new FileDownloadRequest
        {
            Path = path
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});
```

---

# Endpoint completo de signed URL

```csharp
using CustomCodeFramework.Storage.Abstractions;
using CustomCodeFramework.Storage.Security;

app.MapGet("/api/v1/files/signed-url", async (
    string path,
    IFileStorageProvider provider,
    CancellationToken cancellationToken) =>
{
    var url = await provider.GenerateSignedUrlAsync(
        path,
        new SignedUrlOptions
        {
            Permission = FilePermission.Read,
            ExpiresIn = TimeSpan.FromMinutes(15)
        },
        cancellationToken);

    return Results.Ok(new
    {
        Url = url
    });
});
```

---

# Integración con Reports

```text
ReportGenerator
    ↓
ReportFileResult
    ↓
IFileStorage
    ↓
Storage provider
```

---

# Integración con Notifications

```text
IFileStorage.DownloadAsync
    ↓
NotificationAttachment
    ↓
Email
```

---

# Integración con Redis

Cachear URLs firmadas temporalmente:

```csharp
var cacheKey = keyBuilder.Build(
    "storage",
    "signed-url",
    path);

var cachedUrl = await cacheService.GetAsync<string>(
    cacheKey,
    cancellationToken);

if (!string.IsNullOrWhiteSpace(cachedUrl))
{
    return cachedUrl;
}

var url = await provider.GenerateSignedUrlAsync(
    path,
    options,
    cancellationToken);

await cacheService.SetAsync(
    cacheKey,
    url,
    TimeSpan.FromMinutes(5),
    cancellationToken);

return url;
```

---

# Integración con Auth

Proteger endpoints:

```csharp
app.MapPost("/api/v1/files", UploadAsync)
    .RequireAuthorization("permission:storage.upload");

app.MapGet("/api/v1/files/download", DownloadAsync)
    .RequireAuthorization("permission:storage.download");

app.MapDelete("/api/v1/files", DeleteAsync)
    .RequireAuthorization("permission:storage.delete");
```

Permisos recomendados:

```text
storage.upload
storage.download
storage.delete
storage.generate_signed_url
```

---

# Buenas prácticas generales

## Sí

Validar extensión.

Validar content type.

Validar tamaño.

Usar URLs firmadas para archivos privados.

Usar nombres generados, no confiar en nombres del usuario.

Guardar metadata del archivo en base de datos.

Separar provider base del provider concreto.

Usar storage externo en producción distribuida.

---

## No

No guardar archivos privados en wwwroot.

No confiar solo en `file.FileName`.

No guardar secretos en appsettings versionado.

No hacer público un bucket completo.

No subir archivos sin límite de tamaño.

No permitir cualquier extensión.

---

# Errores comunes

## Confiar en extensión

Incorrecto:

```csharp
if (file.FileName.EndsWith(".pdf"))
{
}
```

Correcto:

```text
validar extensión
validar content type
validar firma del archivo si aplica
```

---

## Guardar nombre original como path final

Incorrecto:

```text
uploads/factura.pdf
```

Puede sobrescribir.

Correcto:

```text
uploads/2026/06/09/factura_8f5b4c2a.pdf
```

---

## URLs públicas para archivos privados

Incorrecto:

```text
https://cdn.example.com/private/invoice.pdf
```

Correcto:

```text
signed URL con expiración
```

---

# Resumen

## CustomCodeFramework.Storage

Usar para:

```text
contratos base
uploads
downloads
validaciones
paths
content types
seguridad
URLs firmadas
metadata
```

## CustomCodeFramework.Storage.Local

Usar para:

```text
desarrollo
pruebas
servidores simples
archivos temporales
```

## CustomCodeFramework.Storage.S3

Usar para:

```text
AWS S3
MinIO
storage compatible S3
producción
CDN
```

## CustomCodeFramework.Storage.AzureBlob

Usar para:

```text
Azure
Blob Storage
SAS URLs
```

## CustomCodeFramework.Storage.GoogleCloud

Usar para:

```text
Google Cloud Platform
Google Cloud Storage
signed URLs
```

## CustomCodeFramework.Storage.Firebase

Usar para:

```text
Firebase apps
mobile/web apps
Firebase Storage
```

Combinación recomendada:

```text
Storage -> contratos
Provider concreto -> implementación
Reports -> genera archivos
Notifications -> envía archivos o links
Redis -> cache de signed URLs
Auth -> permisos
Observability -> logs y métricas
```
