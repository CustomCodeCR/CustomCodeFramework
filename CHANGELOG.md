# Changelog

## [0.3.0] — 2026-11-07 (candidate, feature branch only)

- Batch: `rt-20261107-01`, plan Dhole Realtime v2.1.0.
- Additive `CustomCodeFramework.Messaging.Realtime` contracts: `RealtimeEnvelope`, `IOutboxWriter`, `IEventPublisher`, `IInboxStore` and `IRealtimeAudienceResolver`, with minimal payload validation and version-gap reconciliation rules.
- Add 35 xUnit Fact scenarios and 7 theory cases, isolated CI workflow. No existing messaging contracts removed or replaced.
- **Not released**: F0 gate pending, CI outcome must be verified; no merge, NuGet publication or deploy.

## [0.2.0]

- Baseline version observed in `Directory.Build.props` before the realtime batch.
