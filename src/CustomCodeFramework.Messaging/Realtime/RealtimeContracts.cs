using System.Security.Claims;

namespace CustomCodeFramework.Messaging.Realtime;

/// <summary>
/// Appends an event to the service-owned durable Outbox in the SAME transaction
/// as the business mutation. F1 defines only the contract; F2 supplies storage.
/// </summary>
public interface IOutboxWriter
{
    ValueTask AppendAsync(RealtimeEnvelope envelope, CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads already-committed Outbox entries and publishes them to durable Streams.
/// This contract must never be invoked inside a business transaction.
/// </summary>
public interface IEventPublisher
{
    ValueTask PublishAsync(RealtimeEnvelope envelope, CancellationToken cancellationToken = default);
}

/// <summary>
/// Durable consumer deduplication. TryBegin and business effect must be atomic
/// when both use the same database; otherwise use a recoverable state machine.
/// A successful SignalR send is not a business effect or delivery guarantee.
/// </summary>
public interface IInboxStore
{
    ValueTask<bool> TryBeginAsync(Guid eventId, string consumer, CancellationToken cancellationToken = default);
    ValueTask MarkCompletedAsync(Guid eventId, string consumer, CancellationToken cancellationToken = default);
    ValueTask MarkFailedAsync(Guid eventId, string consumer, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolved recipient approved by the server for this resource. Group keys are
/// computed by the transport layer, never supplied directly by the browser.
/// </summary>
public sealed record RealtimeAudience(string Scope, string ResourceId, string RecipientId);

public interface IRealtimeAudienceResolver
{
    ValueTask<IReadOnlyList<RealtimeAudience>> ResolveAuthorizedAsync(
        RealtimeEnvelope envelope,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}

public static class RealtimeSchemaEvolution
{
    /// <summary>
    /// Compatible notifications keep the same major event suffix (vN), do not
    /// repurpose IDs, and may add only optional payload fields. A breaking
    /// payload change must introduce a new event type ending in a new vN.
    /// </summary>
    public static bool CanConsume(
        string expectedEventType,
        int minimumSchemaVersion,
        RealtimeEnvelope envelope) =>
        RealtimeEventType.IsValid(expectedEventType) &&
        string.Equals(expectedEventType, envelope.EventType, StringComparison.Ordinal) &&
        minimumSchemaVersion > 0 &&
        envelope.SchemaVersion >= minimumSchemaVersion;

    /// <summary>
    /// Reject duplicate or stale aggregate versions. Any observed gap must
    /// trigger a REST snapshot rather than synthetic mutations in the browser.
    /// </summary>
    public static RealtimeVersionDecision CompareVersions(
        long currentVersion,
        long incomingVersion)
    {
        if (currentVersion < 0 || incomingVersion <= 0)
            return RealtimeVersionDecision.Invalid;
        if (incomingVersion <= currentVersion)
            return RealtimeVersionDecision.StaleOrDuplicate;
        if (incomingVersion == currentVersion + 1)
            return RealtimeVersionDecision.Next;
        return RealtimeVersionDecision.GapRequiresRestReconciliation;
    }
}

public enum RealtimeVersionDecision
{
    Invalid,
    StaleOrDuplicate,
    Next,
    GapRequiresRestReconciliation
}
