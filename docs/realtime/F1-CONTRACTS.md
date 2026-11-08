# Dhole Realtime universal — F1: contratos (implementación preliminar aislada)

**Batch:** `rt-20261107-01`. **Condición:** el gate F0 todavía debe cerrarse. La implementación F1 NO está validada ni liberada.

## Integración no disruptiva

El namespace nuevo `CustomCodeFramework.Messaging.Realtime` evita romper las interfaces previas de `CustomCodeFramework.Messaging`. `RealtimeEnvelope` es inmutable y lleva eventId/eventType/schemaVersion/aggregateType/aggregateId/aggregateVersion/occurredAtUtc/producer/correlationId/audienceRef/payload/traceId. `AudienceRef` identifica un recurso para autorización exclusivamente en servidor: nunca se usa como nombre de grupo arbitrario enviado por el navegador.

`IOutboxWriter` exige persistencia en la misma transacción que el cambio de negocio (su implementación real, migraciones y prueba de durabilidad son F2). `IEventPublisher` solo publica después del commit; `IInboxStore` establece contratos de idempotencia y recuperación (requieren implementación en F2). `IRealtimeAudienceResolver` recibe `ClaimsPrincipal` y debe comprobar permisos del recurso (implementación de grupos en F3). REST sigue siendo fuente de verdad; las versiones con brechas deben reconciliar por GET.

## Política de payload y versionado

Formato de `eventType`: `crm.account.updated.v1`. Esquema aditivo con `schemaVersion` positivo, nuevo sufijo vN en cambio incompatible. Payload JSON objeto <=4096 bytes y sin nombres de campos personales/secretos comunes (validación defensiva, no sustituye allowlist por evento). Cambios de estado no transmiten entidades completas ni datos sensibles. Serializador camelCase, `occurredAtUtc` con offset UTC cero.

## Prueba de la rama

```bash
dotnet restore tests/CustomCodeFramework.Messaging.Realtime.Tests/CustomCodeFramework.Messaging.Realtime.Tests.csproj
dotnet test tests/CustomCodeFramework.Messaging.Realtime.Tests/CustomCodeFramework.Messaging.Realtime.Tests.csproj -c Release --no-restore
```

Suite escrita: 35 tests `[Fact]` + 7 escenarios `[InlineData]` = **42 ejecuciones previstas**, no verificadas todavía. CI: `.github/workflows/realtime-f1-contract-validation.yml` (si es permitida su ejecución en la rama intermedia). No afirmar PASÓ hasta tener logs verificables; errores y skips bloquean el gate F1.

## Fuera de F1

No se creó Hub global ni bridge ni cola. No hay modificación de REST, bases, endpoints o flujo de negocio. No se habilitan flags ni se despliega. Registrar SHA fijado de paquete, dependencias y pruebas antes de F9.
