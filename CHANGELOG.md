# Changelog

## [0.3.3] - 2026-11-08 (F9 candidate only)

- Sincroniza Framework compartido con versión global propuesta `1.13.12` del batch realtime. PATCH 0.3.2 → 0.3.3; contratos compatibles, sin publicación NuGet y sin integración a develop/master.


## [0.3.2] - 2026-11-08 (candidato F9, sin publicación)

- Alinea PATCH de Framework `0.3.1` → `0.3.2` con el ecosistema Dhole `1.13.11`. Contratos realtime existentes sin cambios, sin publicación NuGet ni cambios a develop/master.


## [0.3.1] - 2026-11-08 (candidato F9, sin publicar NuGet)

- Actualiza versión PATCH del contrato compartido de F1 y su `ecosystemVersion=1.13.10` para el batch `rt-20261107-01`; sin romper APIs, sin publicar paquetes y sin afectar master/develop. La versión global candidata queda coordinada con todos los componentes Dhole.


## [0.3.0] — 2026-11-07 (candidate, feature branch only)

- Batch: `rt-20261107-01`, plan Dhole Realtime v2.1.0.
- Additive `CustomCodeFramework.Messaging.Realtime` contracts: `RealtimeEnvelope`, `IOutboxWriter`, `IEventPublisher`, `IInboxStore` and `IRealtimeAudienceResolver`, with minimal payload validation and version-gap reconciliation rules.
- Add 37 xUnit Fact scenarios and 7 theory cases, isolated CI workflow; 44/44 PASS on run 42485214061 (SHA 86645d36403869ccd791debb384bc298a6b2328a). Includes F2 CRM wire compatibility and rejection of nested PII.
- **Not released**: F1 technical criteria CI-pass but F0/F1 sequential gates pending; no merge, NuGet publication or deploy.

## [0.2.0]

- Baseline version observed in `Directory.Build.props` before the realtime batch.
