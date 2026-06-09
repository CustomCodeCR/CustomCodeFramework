# CustomCodeFramework.Reports

## Introducción

`CustomCodeFramework.Reports` es el paquete base para generación, exportación y administración de reportes.

Este paquete no debe depender de una tecnología concreta para generar archivos.

Su objetivo es definir contratos y modelos comunes para reportes en diferentes formatos:

```text
CSV
Excel
Word
PDF
```

Los paquetes concretos son:

```text
CustomCodeFramework.Reports.Csv
CustomCodeFramework.Reports.Excel
CustomCodeFramework.Reports.Word
CustomCodeFramework.Reports.Pdf
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Reports --version 0.1.0-alpha.1
```

---

# Configuración base

```json
{
  "Reports": {
    "DefaultStorageFolder": "reports",
    "DefaultExpirationDays": 30,
    "StoreGeneratedReports": false,
    "MaxRows": 100000
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Reports.DependencyInjection;

builder.Services.AddCustomCodeReports(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Reports
├── Abstractions
│   ├── IReport.cs
│   ├── IReportGenerator.cs
│   ├── IReportRenderer.cs
│   ├── IReportExporter.cs
│   ├── IReportTemplateProvider.cs
│   └── IReportDataProvider.cs
│
├── Definitions
│   ├── ReportDefinition.cs
│   ├── ReportParameter.cs
│   ├── ReportParameterType.cs
│   ├── ReportColumnDefinition.cs
│   └── ReportFilterDefinition.cs
│
├── Requests
│   ├── ReportRequest.cs
│   ├── ReportGenerationRequest.cs
│   └── ReportExportRequest.cs
│
├── Results
│   ├── ReportResult.cs
│   ├── ReportFileResult.cs
│   ├── ReportGenerationResult.cs
│   └── ReportValidationResult.cs
│
├── Formats
│   ├── ReportFormat.cs
│   ├── ReportContentType.cs
│   └── ReportFileExtension.cs
│
├── Templates
│   ├── ReportTemplate.cs
│   ├── ReportTemplateContext.cs
│   └── ReportTemplateOptions.cs
│
├── Jobs
│   ├── ReportJob.cs
│   ├── ReportJobStatus.cs
│   └── ReportJobResult.cs
│
├── Storage
│   ├── IReportStorage.cs
│   ├── ReportStoragePath.cs
│   └── ReportStoredFile.cs
│
├── Options
│   └── ReportsOptions.cs
│
└── DependencyInjection
    └── ReportServiceCollectionExtensions.cs
```

---

# ¿Qué problema resuelve?

Sin una abstracción común, cada módulo genera reportes de forma diferente:

```csharp
GenerateExcel();
GeneratePdf();
GenerateCsv();
```

Problemas:

```text
lógica duplicada
formatos inconsistentes
reportes difíciles de mantener
sin metadata estándar
sin validación de parámetros
sin tracking de jobs
sin integración con storage
sin integración con notificaciones
```

Con `CustomCodeFramework.Reports`:

```text
Request
    ↓
IReportGenerator
    ↓
IReportDataProvider
    ↓
IReportExporter
    ↓
ReportResult
```

---

# Conceptos principales

## Report

Representa un reporte del sistema.

Ejemplos:

```text
SalesReport
ClientsReport
InvoicesReport
InventoryReport
AuditLogReport
```

---

# IReport

Contrato base para reportes.

Uso conceptual:

```csharp
using CustomCodeFramework.Reports.Abstractions;

public sealed class SalesReport : IReport
{
    public string ReportKey => "sales";

    public string Name => "Sales Report";
}
```

---

# ReportDefinition

Define metadata del reporte.

Ejemplo:

```csharp
using CustomCodeFramework.Reports.Definitions;

var definition = new ReportDefinition
{
    ReportKey = "sales",
    Name = "Sales Report",
    Description = "Report of sales in a date range.",
    Parameters =
    [
        new ReportParameter
        {
            Name = "from",
            Type = ReportParameterType.Date,
            IsRequired = true
        },
        new ReportParameter
        {
            Name = "to",
            Type = ReportParameterType.Date,
            IsRequired = true
        }
    ],
    Columns =
    [
        new ReportColumnDefinition
        {
            Name = "InvoiceNumber",
            Title = "Invoice Number",
            DataType = "string"
        },
        new ReportColumnDefinition
        {
            Name = "ClientName",
            Title = "Client",
            DataType = "string"
        },
        new ReportColumnDefinition
        {
            Name = "Total",
            Title = "Total",
            DataType = "decimal"
        }
    ]
};
```

---

# ReportParameter

Representa un parámetro requerido por un reporte.

```csharp
var parameter = new ReportParameter
{
    Name = "from",
    Label = "From date",
    Type = ReportParameterType.Date,
    IsRequired = true
};
```

---

# ReportParameterType

Tipos comunes:

```text
String
Number
Decimal
Date
DateTime
Boolean
Guid
Enum
```

---

# ReportColumnDefinition

Define columnas.

```csharp
var column = new ReportColumnDefinition
{
    Name = "Total",
    Title = "Total",
    DataType = "decimal",
    Format = "N2"
};
```

---

# ReportFilterDefinition

Define filtros disponibles.

```csharp
var filter = new ReportFilterDefinition
{
    Name = "clientId",
    Label = "Client",
    Type = ReportParameterType.Guid,
    IsRequired = false
};
```

---

# ReportRequest

Request base para pedir datos.

```csharp
using CustomCodeFramework.Reports.Requests;

var request = new ReportRequest
{
    ReportKey = "sales",
    RequestedBy = "user-001",
    Parameters = new Dictionary<string, object?>
    {
        ["from"] = new DateTime(2026, 1, 1),
        ["to"] = new DateTime(2026, 1, 31)
    }
};
```

---

# ReportGenerationRequest

Request para generar archivo.

```csharp
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

var request = new ReportGenerationRequest
{
    ReportKey = "sales",
    Format = ReportFormat.Excel,
    RequestedBy = "user-001",
    Parameters = new Dictionary<string, object?>
    {
        ["from"] = new DateTime(2026, 1, 1),
        ["to"] = new DateTime(2026, 1, 31)
    }
};
```

---

# ReportExportRequest

Request específico para exportar.

```csharp
var exportRequest = new ReportExportRequest
{
    ReportKey = "sales",
    Format = ReportFormat.Pdf,
    Data = rows,
    FileName = "sales-report.pdf"
};
```

---

# ReportFormat

Formatos soportados:

```text
Csv
Excel
Word
Pdf
```

Uso:

```csharp
ReportFormat.Excel
ReportFormat.Pdf
ReportFormat.Csv
ReportFormat.Word
```

---

# ReportContentType

Content types esperados:

```text
text/csv
application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
application/vnd.openxmlformats-officedocument.wordprocessingml.document
application/pdf
```

---

# ReportFileExtension

Extensiones esperadas:

```text
.csv
.xlsx
.docx
.pdf
```

---

# IReportDataProvider

## ¿Para qué sirve?

Obtiene los datos del reporte.

No genera el archivo.

Solo devuelve datos.

---

## Ejemplo

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Requests;

public sealed class SalesReportDataProvider : IReportDataProvider
{
    public string ReportKey => "sales";

    public Task<IReadOnlyCollection<object>> GetDataAsync(
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<object> rows =
        [
            new
            {
                InvoiceNumber = "INV-001",
                ClientName = "ACME",
                IssueDate = new DateTime(2026, 1, 1),
                Subtotal = 1000m,
                Taxes = 130m,
                Total = 1130m
            },
            new
            {
                InvoiceNumber = "INV-002",
                ClientName = "CustomCode",
                IssueDate = new DateTime(2026, 1, 2),
                Subtotal = 2000m,
                Taxes = 260m,
                Total = 2260m
            }
        ];

        return Task.FromResult(rows);
    }
}
```

---

# Data Provider usando Dapper

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Requests;

public sealed class SalesReportDataProvider(
    ISqlQueryExecutor queryExecutor)
    : IReportDataProvider
{
    public string ReportKey => "sales";

    public async Task<IReadOnlyCollection<object>> GetDataAsync(
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var from = (DateTime)request.Parameters["from"]!;
        var to = (DateTime)request.Parameters["to"]!;

        var rows = await queryExecutor.QueryAsync<SalesReportRow>(
            """
            select
                i.invoice_number as InvoiceNumber,
                c.name as ClientName,
                i.issue_date as IssueDate,
                i.subtotal as Subtotal,
                i.taxes as Taxes,
                i.total as Total
            from app.invoices i
            inner join app.clients c on c.client_id = i.client_id
            where i.issue_date >= @From
              and i.issue_date < @To
            order by i.issue_date
            """,
            new
            {
                From = from,
                To = to
            },
            cancellationToken);

        return rows.Cast<object>().ToArray();
    }
}

public sealed record SalesReportRow(
    string InvoiceNumber,
    string ClientName,
    DateTime IssueDate,
    decimal Subtotal,
    decimal Taxes,
    decimal Total);
```

---

# Registrar Data Provider

```csharp
using CustomCodeFramework.Reports.DependencyInjection;

builder.Services
    .AddCustomCodeReports(builder.Configuration)
    .AddReportDataProvider<SalesReportDataProvider>();
```

---

# IReportExporter

## ¿Para qué sirve?

Exporta datos a un formato específico.

Ejemplos de implementaciones:

```text
CsvReportExporter
ExcelReportExporter
WordReportExporter
PdfReportExporter
```

---

## Uso conceptual

```csharp
using CustomCodeFramework.Reports.Abstractions;

public sealed class ReportExportService(
    IReportExporter exporter)
{
    public Task<ReportFileResult> ExportAsync(
        ReportExportRequest request,
        CancellationToken cancellationToken)
    {
        return exporter.ExportAsync(
            request,
            cancellationToken);
    }
}
```

---

# IReportGenerator

## ¿Para qué sirve?

Orquesta todo el flujo del reporte.

```text
validar request
buscar data provider
obtener datos
buscar exporter
generar archivo
guardar opcionalmente
devolver resultado
```

---

## Uso

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

public sealed class GenerateSalesReportService(
    IReportGenerator reportGenerator)
{
    public Task<ReportGenerationResult> GenerateAsync(
        CancellationToken cancellationToken)
    {
        return reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = "sales",
                Format = ReportFormat.Excel,
                RequestedBy = "user-001",
                Parameters = new Dictionary<string, object?>
                {
                    ["from"] = new DateTime(2026, 1, 1),
                    ["to"] = new DateTime(2026, 1, 31)
                }
            },
            cancellationToken);
    }
}
```

---

# Endpoint para generar reporte

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

app.MapPost("/api/v1/reports/sales", async (
    GenerateSalesReportRequest request,
    IReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    var result = await reportGenerator.GenerateAsync(
        new ReportGenerationRequest
        {
            ReportKey = "sales",
            Format = request.Format,
            RequestedBy = request.RequestedBy,
            Parameters = new Dictionary<string, object?>
            {
                ["from"] = request.From,
                ["to"] = request.To
            }
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});

public sealed record GenerateSalesReportRequest(
    DateTime From,
    DateTime To,
    ReportFormat Format,
    string RequestedBy);
```

---

# ReportResult

Resultado base.

```csharp
var result = new ReportResult
{
    IsSuccess = true,
    ReportKey = "sales"
};
```

---

# ReportFileResult

Resultado con archivo.

```csharp
var file = new ReportFileResult
{
    FileName = "sales-report.xlsx",
    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    Content = bytes
};
```

---

# ReportGenerationResult

Resultado completo de generación.

```csharp
var result = new ReportGenerationResult
{
    ReportKey = "sales",
    Format = ReportFormat.Excel,
    FileName = "sales-report.xlsx",
    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    Content = bytes,
    GeneratedAtUtc = DateTime.UtcNow
};
```

---

# ReportValidationResult

Resultado de validación de parámetros.

```csharp
var validation = new ReportValidationResult
{
    IsValid = false,
    Errors =
    [
        "Parameter from is required.",
        "Parameter to is required."
    ]
};
```

---

# Templates

## ReportTemplate

```csharp
var template = new ReportTemplate
{
    TemplateKey = "sales-pdf",
    Content = "<h1>{{title}}</h1><table>{{rows}}</table>"
};
```

---

## ReportTemplateContext

```csharp
var context = new ReportTemplateContext
{
    Data = new Dictionary<string, object?>
    {
        ["title"] = "Sales Report",
        ["generatedAt"] = DateTime.UtcNow,
        ["rows"] = rows
    }
};
```

---

# Report Jobs

Los jobs sirven para reportes pesados o asincrónicos.

---

# ReportJob

```csharp
var job = new ReportJob
{
    ReportJobId = Guid.NewGuid(),
    ReportKey = "sales",
    Format = ReportFormat.Excel,
    Status = ReportJobStatus.Pending,
    RequestedBy = "user-001",
    CreatedAtUtc = DateTime.UtcNow
};
```

---

# ReportJobStatus

Estados:

```text
Pending
Running
Completed
Failed
Cancelled
```

---

# ReportJobResult

```csharp
var result = new ReportJobResult
{
    ReportJobId = job.ReportJobId,
    Status = ReportJobStatus.Completed,
    FileName = "sales-report.xlsx",
    StoragePath = "reports/sales/sales-report.xlsx",
    CompletedAtUtc = DateTime.UtcNow
};
```

---

# IReportStorage

Abstracción para guardar reportes.

Puede integrarse con:

```text
CustomCodeFramework.Storage
Local
S3
Azure Blob
Google Cloud Storage
Firebase
```

---

## Uso conceptual

```csharp
using CustomCodeFramework.Reports.Storage;

public sealed class ReportStorageService(
    IReportStorage reportStorage)
{
    public Task<ReportStoredFile> StoreAsync(
        ReportFileResult file,
        CancellationToken cancellationToken)
    {
        return reportStorage.StoreAsync(
            file,
            cancellationToken);
    }
}
```

---

# ReportStoragePath

```csharp
var path = new ReportStoragePath
{
    Folder = "reports/sales",
    FileName = "sales-report.xlsx"
};
```

---

# ReportStoredFile

```csharp
var storedFile = new ReportStoredFile
{
    FileName = "sales-report.xlsx",
    Path = "reports/sales/sales-report.xlsx",
    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    SizeInBytes = 10240,
    CreatedAtUtc = DateTime.UtcNow
};
```

---

# Ejemplo completo: Sales Report

## DTO

```csharp
public sealed record SalesReportRow(
    string InvoiceNumber,
    string ClientName,
    DateTime IssueDate,
    decimal Subtotal,
    decimal Taxes,
    decimal Total);
```

---

## Data Provider

```csharp
using CustomCodeFramework.Postgres.Dapper.Abstractions;
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Requests;

public sealed class SalesReportDataProvider(
    ISqlQueryExecutor queryExecutor)
    : IReportDataProvider
{
    public string ReportKey => "sales";

    public async Task<IReadOnlyCollection<object>> GetDataAsync(
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var from = (DateTime)request.Parameters["from"]!;
        var to = (DateTime)request.Parameters["to"]!;

        var rows = await queryExecutor.QueryAsync<SalesReportRow>(
            """
            select
                i.invoice_number as InvoiceNumber,
                c.name as ClientName,
                i.issue_date as IssueDate,
                i.subtotal as Subtotal,
                i.taxes as Taxes,
                i.total as Total
            from app.invoices i
            inner join app.clients c on c.client_id = i.client_id
            where i.issue_date >= @From
              and i.issue_date < @To
            order by i.issue_date
            """,
            new
            {
                From = from,
                To = to
            },
            cancellationToken);

        return rows.Cast<object>().ToArray();
    }
}
```

---

## Endpoint

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

app.MapPost("/api/v1/reports/sales", async (
    GenerateSalesReportRequest request,
    IReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    var result = await reportGenerator.GenerateAsync(
        new ReportGenerationRequest
        {
            ReportKey = "sales",
            Format = request.Format,
            RequestedBy = request.RequestedBy,
            Parameters = new Dictionary<string, object?>
            {
                ["from"] = request.From,
                ["to"] = request.To
            }
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});

public sealed record GenerateSalesReportRequest(
    DateTime From,
    DateTime To,
    ReportFormat Format,
    string RequestedBy);
```

---

# CustomCodeFramework.Reports.Csv

## Introducción

`CustomCodeFramework.Reports.Csv` exporta reportes en formato `.csv`.

Se recomienda para:

```text
integraciones simples
exportaciones ligeras
datos tabulares
archivos para importar en otros sistemas
reportes sin estilos
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Reports.Csv --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Reports": {
    "Csv": {
      "Delimiter": "Comma",
      "IncludeHeader": true,
      "UseUtf8Bom": true
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Reports.DependencyInjection;
using CustomCodeFramework.Reports.Csv.DependencyInjection;

builder.Services
    .AddCustomCodeReports(builder.Configuration)
    .AddCustomCodeCsvReports(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Reports.Csv
├── Exporting
│   ├── CsvReportExporter.cs
│   └── CsvExportOptions.cs
├── Formatting
│   ├── CsvDelimiter.cs
│   └── CsvColumnFormatter.cs
└── DependencyInjection
    └── CsvReportServiceCollectionExtensions.cs
```

---

# CsvExportOptions

```csharp
var options = new CsvExportOptions
{
    Delimiter = CsvDelimiter.Comma,
    IncludeHeader = true,
    UseUtf8Bom = true
};
```

---

# Delimiters

```text
Comma      -> ,
Semicolon  -> ;
Tab        -> \t
Pipe       -> |
```

---

# Exportar CSV con IReportGenerator

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

public sealed class CsvSalesReportService(
    IReportGenerator reportGenerator)
{
    public Task<ReportGenerationResult> GenerateAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = "sales",
                Format = ReportFormat.Csv,
                RequestedBy = "user-001",
                Parameters = new Dictionary<string, object?>
                {
                    ["from"] = from,
                    ["to"] = to
                }
            },
            cancellationToken);
    }
}
```

---

# Endpoint CSV

```csharp
app.MapGet("/api/v1/reports/sales.csv", async (
    DateTime from,
    DateTime to,
    IReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    var result = await reportGenerator.GenerateAsync(
        new ReportGenerationRequest
        {
            ReportKey = "sales",
            Format = ReportFormat.Csv,
            RequestedBy = "system",
            Parameters = new Dictionary<string, object?>
            {
                ["from"] = from,
                ["to"] = to
            }
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});
```

---

# CSV esperado

```csv
InvoiceNumber,ClientName,IssueDate,Subtotal,Taxes,Total
INV-001,ACME,2026-01-01,1000.00,130.00,1130.00
INV-002,CustomCode,2026-01-02,2000.00,260.00,2260.00
```

---

# Buenas prácticas CSV

## Sí

Usar encabezados.

Usar UTF-8 BOM si se abre en Excel.

Usar punto decimal si es para integraciones.

Usar delimitador `;` si el Excel regional usa coma decimal.

---

## No

No usar CSV para reportes con estilos.

No usar CSV si se necesitan múltiples hojas.

No meter HTML dentro de CSV.

---

# CustomCodeFramework.Reports.Excel

## Introducción

`CustomCodeFramework.Reports.Excel` exporta reportes en `.xlsx`.

Se recomienda para:

```text
reportes tabulares
reportes con estilos
múltiples hojas
totales
filtros
reportes financieros
exportaciones para usuarios
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Reports.Excel --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Reports": {
    "Excel": {
      "DefaultWorksheetName": "Report",
      "AutoAdjustColumns": true,
      "FreezeHeaderRow": true,
      "UseTable": true,
      "IncludeHeader": true
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Reports.DependencyInjection;
using CustomCodeFramework.Reports.Excel.DependencyInjection;

builder.Services
    .AddCustomCodeReports(builder.Configuration)
    .AddCustomCodeExcelReports(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Reports.Excel
├── Exporting
│   ├── ExcelReportExporter.cs
│   └── ExcelExportOptions.cs
├── Styling
│   ├── ExcelStyleOptions.cs
│   └── ExcelColumnStyle.cs
├── Worksheets
│   ├── ExcelWorksheetDefinition.cs
│   └── ExcelWorksheetBuilder.cs
└── DependencyInjection
    └── ExcelReportServiceCollectionExtensions.cs
```

---

# ExcelExportOptions

```csharp
var options = new ExcelExportOptions
{
    DefaultWorksheetName = "Sales",
    AutoAdjustColumns = true,
    FreezeHeaderRow = true,
    UseTable = true,
    IncludeHeader = true
};
```

---

# ExcelColumnStyle

```csharp
var totalColumnStyle = new ExcelColumnStyle
{
    ColumnName = "Total",
    NumberFormat = "#,##0.00",
    Width = 15
};
```

---

# ExcelWorksheetDefinition

```csharp
var worksheet = new ExcelWorksheetDefinition
{
    Name = "Sales",
    Columns =
    [
        new ReportColumnDefinition
        {
            Name = "InvoiceNumber",
            Title = "Invoice Number",
            DataType = "string"
        },
        new ReportColumnDefinition
        {
            Name = "Total",
            Title = "Total",
            DataType = "decimal",
            Format = "#,##0.00"
        }
    ]
};
```

---

# Generar Excel

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

public sealed class ExcelSalesReportService(
    IReportGenerator reportGenerator)
{
    public Task<ReportGenerationResult> GenerateAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = "sales",
                Format = ReportFormat.Excel,
                RequestedBy = "user-001",
                Parameters = new Dictionary<string, object?>
                {
                    ["from"] = from,
                    ["to"] = to,
                    ["title"] = "Sales Report"
                }
            },
            cancellationToken);
    }
}
```

---

# Endpoint Excel

```csharp
app.MapGet("/api/v1/reports/sales.xlsx", async (
    DateTime from,
    DateTime to,
    IReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    var result = await reportGenerator.GenerateAsync(
        new ReportGenerationRequest
        {
            ReportKey = "sales",
            Format = ReportFormat.Excel,
            RequestedBy = "system",
            Parameters = new Dictionary<string, object?>
            {
                ["from"] = from,
                ["to"] = to
            }
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});
```

---

# Excel con múltiples hojas

```csharp
var workbook = new[]
{
    new ExcelWorksheetDefinition
    {
        Name = "Sales",
        Data = salesRows
    },
    new ExcelWorksheetDefinition
    {
        Name = "Summary",
        Data = summaryRows
    }
};
```

---

# Buenas prácticas Excel

## Sí

Usar para reportes que abre un usuario.

Aplicar formato numérico.

Congelar encabezado.

Auto ajustar columnas.

Usar múltiples hojas si hay resumen y detalle.

---

## No

No usar Excel para integraciones automatizadas si CSV basta.

No generar archivos enormes sin paginar o streaming.

No meter datos sensibles innecesarios.

---

# CustomCodeFramework.Reports.Word

## Introducción

`CustomCodeFramework.Reports.Word` exporta documentos `.docx`.

Se recomienda para:

```text
contratos
cartas
certificaciones
documentos administrativos
propuestas
documentos con secciones
documentos que el usuario debe editar
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Reports.Word --version 0.1.0-alpha.1
```

---

# Configuración

```json
{
  "Reports": {
    "Word": {
      "DefaultTitle": "Report",
      "DefaultAuthor": "CustomCodeFramework",
      "IncludeGeneratedAt": true,
      "IncludeParameters": true,
      "IncludeTableHeader": true
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Reports.DependencyInjection;
using CustomCodeFramework.Reports.Word.DependencyInjection;

builder.Services
    .AddCustomCodeReports(builder.Configuration)
    .AddCustomCodeWordReports(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Reports.Word
├── Exporting
│   ├── WordReportExporter.cs
│   └── WordExportOptions.cs
├── Templates
│   ├── WordTemplate.cs
│   └── WordTemplateRenderer.cs
├── Sections
│   ├── WordSection.cs
│   └── WordSectionBuilder.cs
└── DependencyInjection
    └── WordReportServiceCollectionExtensions.cs
```

---

# WordExportOptions

```csharp
var options = new WordExportOptions
{
    DefaultTitle = "Sales Report",
    DefaultAuthor = "CustomCodeFramework",
    IncludeGeneratedAt = true,
    IncludeParameters = true,
    IncludeTableHeader = true
};
```

---

# WordTemplate

```csharp
var template = new WordTemplate
{
    TemplateKey = "client-contract",
    Title = "Client Contract",
    Content = """
    Contract for {{clientName}}

    Total amount: {{total}}
    """
};
```

---

# WordSection

```csharp
var section = new WordSection
{
    Title = "Sales Summary",
    Content = "This section contains the sales summary."
};
```

---

# Generar Word

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

public sealed class WordSalesReportService(
    IReportGenerator reportGenerator)
{
    public Task<ReportGenerationResult> GenerateAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = "sales",
                Format = ReportFormat.Word,
                RequestedBy = "user-001",
                Parameters = new Dictionary<string, object?>
                {
                    ["from"] = from,
                    ["to"] = to,
                    ["title"] = "Sales Report"
                }
            },
            cancellationToken);
    }
}
```

---

# Endpoint Word

```csharp
app.MapGet("/api/v1/reports/sales.docx", async (
    DateTime from,
    DateTime to,
    IReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    var result = await reportGenerator.GenerateAsync(
        new ReportGenerationRequest
        {
            ReportKey = "sales",
            Format = ReportFormat.Word,
            RequestedBy = "system",
            Parameters = new Dictionary<string, object?>
            {
                ["from"] = from,
                ["to"] = to
            }
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});
```

---

# Ejemplo: Documento de contrato

```csharp
public sealed record ContractDocumentData(
    string ClientName,
    string Identification,
    string Address,
    DateTime ContractDate,
    decimal Amount);
```

```csharp
public sealed class ContractDocumentDataProvider : IReportDataProvider
{
    public string ReportKey => "client-contract";

    public Task<IReadOnlyCollection<object>> GetDataAsync(
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<object> data =
        [
            new ContractDocumentData(
                "ACME",
                "3-101-999999",
                "San José, Costa Rica",
                DateTime.UtcNow,
                1500000m)
        ];

        return Task.FromResult(data);
    }
}
```

---

# Buenas prácticas Word

## Sí

Usar para documentos editables.

Usar templates.

Separar datos del formato.

Usar secciones.

Mantener estilos consistentes.

---

## No

No usar Word para reportes tabulares grandes.

No usar Word si el archivo no debe editarse.

Para documentos finales no editables, usar PDF.

---

# CustomCodeFramework.Reports.Pdf

## Introducción

`CustomCodeFramework.Reports.Pdf` exporta reportes en `.pdf`.

Este paquete usa `DinkToPdf`.

Se recomienda para:

```text
facturas
cotizaciones
recibos
reportes formales
documentos no editables
documentos para enviar a clientes
comprobantes
```

---

# Instalación

```bash
dotnet add package CustomCodeFramework.Reports.Pdf --version 0.1.0-alpha.1
```

---

# Dependencia nativa

DinkToPdf usa wkhtmltopdf/libwkhtmltox.

En Linux puede requerir:

```bash
sudo pacman -S wkhtmltopdf
```

En servidores Docker se recomienda instalar las dependencias nativas en la imagen.

---

# Configuración

```json
{
  "Reports": {
    "Pdf": {
      "DefaultTitle": "Report",
      "DefaultAuthor": "CustomCodeFramework",
      "IncludeGeneratedAt": true,
      "IncludeParameters": true,
      "IncludeTableHeader": true,
      "UseLandscape": false,
      "PaperSize": "A4",
      "MarginTop": "10mm",
      "MarginBottom": "10mm",
      "MarginLeft": "10mm",
      "MarginRight": "10mm",
      "EnableLocalFileAccess": true,
      "PrintBackground": true,
      "Dpi": 300
    }
  }
}
```

---

# Registro en DI

```csharp
using CustomCodeFramework.Reports.DependencyInjection;
using CustomCodeFramework.Reports.Pdf.DependencyInjection;

builder.Services
    .AddCustomCodeReports(builder.Configuration)
    .AddCustomCodePdfReports(builder.Configuration);
```

---

# Estructura

```text
CustomCodeFramework.Reports.Pdf
├── Exporting
│   ├── PdfReportExporter.cs
│   └── PdfExportOptions.cs
├── Rendering
│   ├── PdfRenderer.cs
│   └── PdfRenderOptions.cs
├── Templates
│   ├── PdfTemplate.cs
│   └── PdfTemplateRenderer.cs
└── DependencyInjection
    └── PdfReportServiceCollectionExtensions.cs
```

---

# PdfExportOptions

```csharp
var options = new PdfExportOptions
{
    PaperSize = "A4",
    UseLandscape = false,
    MarginTop = "10mm",
    MarginBottom = "10mm",
    MarginLeft = "10mm",
    MarginRight = "10mm",
    PrintBackground = true,
    Dpi = 300
};
```

---

# PdfRenderOptions

```csharp
var renderOptions = new PdfRenderOptions
{
    EnableLocalFileAccess = true,
    PrintBackground = true,
    UseLandscape = false
};
```

---

# PdfTemplate

```csharp
var template = new PdfTemplate
{
    TemplateKey = "invoice-pdf",
    Html = """
    <html>
      <body>
        <h1>Invoice {{invoiceNumber}}</h1>
        <p>Total: {{total}}</p>
      </body>
    </html>
    """
};
```

---

# Generar PDF

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

public sealed class PdfSalesReportService(
    IReportGenerator reportGenerator)
{
    public Task<ReportGenerationResult> GenerateAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = "sales",
                Format = ReportFormat.Pdf,
                RequestedBy = "user-001",
                Parameters = new Dictionary<string, object?>
                {
                    ["from"] = from,
                    ["to"] = to,
                    ["title"] = "Sales Report"
                }
            },
            cancellationToken);
    }
}
```

---

# Endpoint PDF

```csharp
app.MapGet("/api/v1/reports/sales.pdf", async (
    DateTime from,
    DateTime to,
    IReportGenerator reportGenerator,
    CancellationToken cancellationToken) =>
{
    var result = await reportGenerator.GenerateAsync(
        new ReportGenerationRequest
        {
            ReportKey = "sales",
            Format = ReportFormat.Pdf,
            RequestedBy = "system",
            Parameters = new Dictionary<string, object?>
            {
                ["from"] = from,
                ["to"] = to
            }
        },
        cancellationToken);

    return Results.File(
        result.Content,
        result.ContentType,
        result.FileName);
});
```

---

# Ejemplo: Factura PDF

## DTO

```csharp
public sealed record InvoicePdfData(
    string InvoiceNumber,
    string ClientName,
    DateTime IssueDate,
    decimal Subtotal,
    decimal Taxes,
    decimal Total,
    IReadOnlyCollection<InvoicePdfLine> Lines);

public sealed record InvoicePdfLine(
    string ProductCode,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total);
```

---

## Data Provider

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Requests;

public sealed class InvoicePdfDataProvider : IReportDataProvider
{
    public string ReportKey => "invoice-pdf";

    public Task<IReadOnlyCollection<object>> GetDataAsync(
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoiceId = (Guid)request.Parameters["invoiceId"]!;

        IReadOnlyCollection<object> data =
        [
            new InvoicePdfData(
                "INV-001",
                "ACME",
                DateTime.UtcNow,
                1000m,
                130m,
                1130m,
                [
                    new InvoicePdfLine(
                        "P001",
                        "Product 1",
                        2,
                        500m,
                        1000m)
                ])
        ];

        return Task.FromResult(data);
    }
}
```

---

## Generar factura

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Formats;
using CustomCodeFramework.Reports.Requests;

public sealed class InvoicePdfService(
    IReportGenerator reportGenerator)
{
    public Task<ReportGenerationResult> GenerateAsync(
        Guid invoiceId,
        string userId,
        CancellationToken cancellationToken)
    {
        return reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = "invoice-pdf",
                Format = ReportFormat.Pdf,
                RequestedBy = userId,
                Parameters = new Dictionary<string, object?>
                {
                    ["invoiceId"] = invoiceId
                }
            },
            cancellationToken);
    }
}
```

---

# HTML recomendado para PDF

```html
<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <style>
      body {
        font-family: Arial, sans-serif;
        font-size: 12px;
      }

      h1 {
        font-size: 20px;
      }

      table {
        width: 100%;
        border-collapse: collapse;
      }

      th,
      td {
        border: 1px solid #dddddd;
        padding: 6px;
      }

      th {
        background: #f2f2f2;
      }

      .text-right {
        text-align: right;
      }
    </style>
  </head>
  <body>
    <h1>Invoice {{invoiceNumber}}</h1>

    <p>Client: {{clientName}}</p>
    <p>Date: {{issueDate}}</p>

    <table>
      <thead>
        <tr>
          <th>Code</th>
          <th>Description</th>
          <th class="text-right">Quantity</th>
          <th class="text-right">Unit Price</th>
          <th class="text-right">Total</th>
        </tr>
      </thead>
      <tbody>
        {{lines}}
      </tbody>
    </table>

    <h3 class="text-right">Total: {{total}}</h3>
  </body>
</html>
```

---

# Buenas prácticas PDF

## Sí

Usar HTML simple.

Usar CSS compatible con wkhtmltopdf.

Usar tamaños y márgenes claros.

Usar PDF para documentos finales.

Probar en Linux si se va a desplegar en Linux.

---

## No

No usar JavaScript pesado.

No depender de CSS moderno no soportado por wkhtmltopdf.

No usar rutas locales sin habilitar local file access.

No generar PDF enorme en request HTTP si puede ser job.

---

# Reportes asíncronos

Para reportes pesados se recomienda:

```text
1. Crear ReportJob
2. Procesar en background worker
3. Guardar archivo en Storage
4. Notificar al usuario
5. Descargar desde URL
```

---

# Flujo recomendado

```text
POST /reports/sales/jobs
    ↓
CreateReportJobCommand
    ↓
ReportJob Pending
    ↓
Background Worker
    ↓
IReportGenerator
    ↓
IReportStorage
    ↓
Notification
```

---

# Ejemplo ReportJob

```csharp
public sealed record CreateReportJobCommand(
    string ReportKey,
    ReportFormat Format,
    Dictionary<string, object?> Parameters) : ICommand<Guid>;
```

---

# Handler

```csharp
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Reports.Jobs;

public sealed class CreateReportJobCommandHandler
    : ICommandHandler<CreateReportJobCommand, Guid>
{
    public Task<Guid> HandleAsync(
        CreateReportJobCommand command,
        CancellationToken cancellationToken = default)
    {
        var jobId = Guid.NewGuid();

        var job = new ReportJob
        {
            ReportJobId = jobId,
            ReportKey = command.ReportKey,
            Format = command.Format,
            Status = ReportJobStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Guardar job.

        return Task.FromResult(jobId);
    }
}
```

---

# Worker

```csharp
using CustomCodeFramework.Reports.Abstractions;
using CustomCodeFramework.Reports.Requests;

public sealed class ReportJobWorker(
    IReportGenerator reportGenerator,
    IReportStorage reportStorage,
    INotificationSender notificationSender)
{
    public async Task ProcessAsync(
        ReportJob job,
        CancellationToken cancellationToken)
    {
        var result = await reportGenerator.GenerateAsync(
            new ReportGenerationRequest
            {
                ReportKey = job.ReportKey,
                Format = job.Format,
                RequestedBy = job.RequestedBy,
                Parameters = job.Parameters
            },
            cancellationToken);

        var storedFile = await reportStorage.StoreAsync(
            new ReportFileResult
            {
                FileName = result.FileName,
                ContentType = result.ContentType,
                Content = result.Content
            },
            cancellationToken);

        // Marcar job como Completed.

        await notificationSender.SendAsync(
            new NotificationMessage
            {
                Channel = NotificationChannelType.SignalR,
                Recipient = new NotificationRecipient
                {
                    UserId = job.RequestedBy
                },
                Subject = "Report ready",
                Body = $"Report {job.ReportKey} is ready.",
                Data = new Dictionary<string, object?>
                {
                    ["path"] = storedFile.Path
                }
            },
            cancellationToken);
    }
}
```

---

# Program.cs con todos los formatos

```csharp
using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Postgres.Dapper.DependencyInjection;
using CustomCodeFramework.Reports.DependencyInjection;
using CustomCodeFramework.Reports.Csv.DependencyInjection;
using CustomCodeFramework.Reports.Excel.DependencyInjection;
using CustomCodeFramework.Reports.Word.DependencyInjection;
using CustomCodeFramework.Reports.Pdf.DependencyInjection;
using CustomCodeFramework.Postgres.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCustomCodeApiWithSwagger(
    title: "Reports API",
    version: "v1");

builder.Services.AddCustomCodePostgres(builder.Configuration);
builder.Services.AddCustomCodePostgresDapper(builder.Configuration);

builder.Services
    .AddCustomCodeReports(builder.Configuration)
    .AddCustomCodeCsvReports(builder.Configuration)
    .AddCustomCodeExcelReports(builder.Configuration)
    .AddCustomCodeWordReports(builder.Configuration)
    .AddCustomCodePdfReports(builder.Configuration)
    .AddReportDataProvider<SalesReportDataProvider>()
    .AddReportDataProvider<InvoicePdfDataProvider>();

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

# appsettings.json completo

```json
{
  "Reports": {
    "DefaultStorageFolder": "reports",
    "DefaultExpirationDays": 30,
    "StoreGeneratedReports": false,
    "MaxRows": 100000,
    "Csv": {
      "Delimiter": "Comma",
      "IncludeHeader": true,
      "UseUtf8Bom": true
    },
    "Excel": {
      "DefaultWorksheetName": "Report",
      "AutoAdjustColumns": true,
      "FreezeHeaderRow": true,
      "UseTable": true,
      "IncludeHeader": true
    },
    "Word": {
      "DefaultTitle": "Report",
      "DefaultAuthor": "CustomCodeFramework",
      "IncludeGeneratedAt": true,
      "IncludeParameters": true,
      "IncludeTableHeader": true
    },
    "Pdf": {
      "DefaultTitle": "Report",
      "DefaultAuthor": "CustomCodeFramework",
      "IncludeGeneratedAt": true,
      "IncludeParameters": true,
      "IncludeTableHeader": true,
      "UseLandscape": false,
      "PaperSize": "A4",
      "MarginTop": "10mm",
      "MarginBottom": "10mm",
      "MarginLeft": "10mm",
      "MarginRight": "10mm",
      "EnableLocalFileAccess": true,
      "PrintBackground": true,
      "Dpi": 300
    }
  }
}
```

---

# Cuándo usar cada formato

## CSV

Usar para:

```text
integraciones
exportación simple
archivos livianos
datos planos
```

## Excel

Usar para:

```text
reportes tabulares
usuarios finales
múltiples hojas
estilos
totales
```

## Word

Usar para:

```text
contratos
cartas
documentos editables
propuestas
certificaciones
```

## PDF

Usar para:

```text
facturas
cotizaciones
recibos
documentos finales
documentos no editables
envío a clientes
```

---

# Integración con Storage

Para guardar reportes:

```text
IReportGenerator
    ↓
ReportFileResult
    ↓
IReportStorage
    ↓
CustomCodeFramework.Storage
```

Ejemplo:

```csharp
var result = await reportGenerator.GenerateAsync(
    request,
    cancellationToken);

var stored = await reportStorage.StoreAsync(
    new ReportFileResult
    {
        FileName = result.FileName,
        ContentType = result.ContentType,
        Content = result.Content
    },
    cancellationToken);
```

---

# Integración con Notifications

Cuando un reporte está listo:

```text
Report generated
    ↓
Storage
    ↓
Notification
```

Ejemplo:

```csharp
await notificationSender.SendAsync(
    new NotificationMessage
    {
        Channel = NotificationChannelType.Email,
        Recipient = new NotificationRecipient
        {
            Email = userEmail
        },
        Subject = "Report ready",
        Body = "Your report is ready.",
        Attachments =
        [
            new NotificationAttachment
            {
                FileName = result.FileName,
                ContentType = result.ContentType,
                Content = result.Content
            }
        ]
    },
    cancellationToken);
```

---

# Integración con Redis

Usar Redis para evitar generar el mismo reporte varias veces al mismo tiempo.

```csharp
await using var handle = await distributedLock.AcquireAsync(
    $"locks:reports:{request.ReportKey}:{request.RequestedBy}",
    TimeSpan.FromMinutes(5),
    cancellationToken);

if (handle is null)
{
    throw new InvalidOperationException("Report is already being generated.");
}
```

---

# Integración con Observability

Métricas recomendadas:

```text
reports_generated_total
reports_failed_total
report_generation_duration_ms
reports_by_format_total
```

Logging recomendado:

```csharp
logger.LogInformation(
    "Generating report {ReportKey} in format {Format}",
    request.ReportKey,
    request.Format);
```

---

# Buenas prácticas generales

## Sí

Separar data provider del exporter.

Usar Dapper para reportes SQL.

Usar jobs para reportes pesados.

Guardar reportes grandes en Storage.

Notificar al usuario cuando termina.

Usar PDF para documentos finales.

Usar Excel para reportes editables/tabulares.

Usar CSV para integraciones.

---

## No

No meter SQL dentro del exporter.

No generar reportes pesados directamente en request HTTP.

No usar PDF para datos que el usuario necesita manipular.

No usar Word para reportes masivos.

No mandar archivos grandes por email si puede usarse link de descarga.

No usar entidades EF directamente como data rows.

---

# Errores comunes

## Mezclar obtención de datos y exportación

Incorrecto:

```csharp
public sealed class ExcelReportExporter
{
    public async Task ExportAsync()
    {
        var rows = await db.QueryAsync(...);
        // exportar
    }
}
```

Correcto:

```text
IReportDataProvider -> obtiene datos
IReportExporter -> genera archivo
```

---

## Usar Excel para integración

Si es integración automatizada, normalmente mejor:

```text
CSV
JSON
API
```

---

## Generar PDFs enormes en HTTP

Incorrecto:

```text
request HTTP espera 5 minutos
```

Correcto:

```text
ReportJob + Worker + Storage + Notification
```

---

## No validar parámetros

Incorrecto:

```csharp
var from = (DateTime)request.Parameters["from"];
```

sin verificar.

Correcto:

```text
ReportValidationResult
ReportParameter
validación antes de generar
```

---

# Resumen

## CustomCodeFramework.Reports

Usar para:

```text
contratos base
data providers
generador
requests
results
formats
jobs
templates
storage de reportes
```

## CustomCodeFramework.Reports.Csv

Usar para:

```text
datos planos
integraciones
exportación simple
```

## CustomCodeFramework.Reports.Excel

Usar para:

```text
reportes tabulares
usuarios finales
formatos
múltiples hojas
```

## CustomCodeFramework.Reports.Word

Usar para:

```text
documentos editables
contratos
cartas
certificaciones
```

## CustomCodeFramework.Reports.Pdf

Usar para:

```text
facturas
cotizaciones
recibos
documentos finales
documentos no editables
```

Combinación recomendada:

```text
Dapper -> obtiene datos
Reports -> orquesta
Csv/Excel/Word/Pdf -> exporta
Storage -> guarda
Notifications -> avisa
Redis -> evita duplicados
Observability -> mide y registra
```
