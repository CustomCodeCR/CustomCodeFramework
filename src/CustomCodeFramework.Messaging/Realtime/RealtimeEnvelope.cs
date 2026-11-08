using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CustomCodeFramework.Messaging.Realtime;

/// <summary>
/// Immutable notification of a committed business change. REST remains authoritative.
/// AudienceRef is a server-side resource reference; it is NOT a SignalR group name.
/// Do not serialize entity snapshots, credentials, or personal information.
/// </summary>
public sealed record RealtimeEnvelope
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required int SchemaVersion { get; init; }
    public required string AggregateType { get; init; }
    public required string AggregateId { get; init; }
    public required long AggregateVersion { get; init; }
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public required string Producer { get; init; }
    public required string CorrelationId { get; init; }
    public required string AudienceRef { get; init; }
    public required JsonElement Payload { get; init; }
    public string? TraceId { get; init; }
}

public static class RealtimeEventType
{
    // Examples: crm.account.updated.v1, pricing.quote.approved.v2.
    private static readonly Regex Format = new(
        @"^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*){2,}\.v[1-9][0-9]*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 160 && Format.IsMatch(value);
}

/// <summary>
/// Safety validation for minimal invalidation events. This is defense-in-depth,
/// not a substitute for authorization or a service-specific payload allowlist.
/// </summary>
public static class RealtimeEnvelopeValidator
{
    public const int MaximumPayloadBytes = 4096;

    private static readonly HashSet<string> ForbiddenNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordHash", "secret", "apiKey", "accessToken",
        "refreshToken", "token", "authorization", "cookie", "email",
        "phone", "phoneNumber", "address", "fullName", "firstName",
        "lastName", "ssn", "taxId", "cardNumber", "creditCard",
        "bankAccount", "identityNumber", "rawPayload", "privateKey"
    };

    public static IReadOnlyList<string> Validate(RealtimeEnvelope? envelope)
    {
        if (envelope is null) return new[] { "envelope.required" };

        var errors = new List<string>();
        if (envelope.EventId == Guid.Empty) errors.Add("eventId.required");
        if (!RealtimeEventType.IsValid(envelope.EventType)) errors.Add("eventType.invalid");
        if (envelope.SchemaVersion <= 0) errors.Add("schemaVersion.invalid");
        if (string.IsNullOrWhiteSpace(envelope.AggregateType) ||
            envelope.AggregateType.Length > 100) errors.Add("aggregateType.invalid");
        if (string.IsNullOrWhiteSpace(envelope.AggregateId) ||
            envelope.AggregateId.Length > 160) errors.Add("aggregateId.invalid");
        if (envelope.AggregateVersion <= 0) errors.Add("aggregateVersion.invalid");
        if (envelope.OccurredAtUtc == default ||
            envelope.OccurredAtUtc.Offset != TimeSpan.Zero) errors.Add("occurredAtUtc.invalid");
        if (string.IsNullOrWhiteSpace(envelope.Producer) ||
            envelope.Producer.Length > 120) errors.Add("producer.invalid");
        if (string.IsNullOrWhiteSpace(envelope.CorrelationId) ||
            envelope.CorrelationId.Length > 160) errors.Add("correlationId.invalid");
        if (string.IsNullOrWhiteSpace(envelope.AudienceRef) ||
            envelope.AudienceRef.Length > 160) errors.Add("audienceRef.invalid");
        if (envelope.TraceId?.Length > 160) errors.Add("traceId.invalid");

        if (envelope.Payload.ValueKind != JsonValueKind.Object)
        {
            errors.Add("payload.objectRequired");
        }
        else
        {
            if (Encoding.UTF8.GetByteCount(envelope.Payload.GetRawText()) > MaximumPayloadBytes)
                errors.Add("payload.tooLarge");
            ScanProperties(envelope.Payload, errors);
        }

        return errors;
    }

    private static void ScanProperties(JsonElement item, List<string> errors)
    {
        switch (item.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in item.EnumerateObject())
                {
                    if (ForbiddenNames.Contains(property.Name))
                        errors.Add($"payload.forbiddenField:{property.Name}");
                    ScanProperties(property.Value, errors);
                }
                break;
            case JsonValueKind.Array:
                foreach (var element in item.EnumerateArray()) ScanProperties(element, errors);
                break;
        }
    }
}

public static class RealtimeEnvelopeCodec
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(RealtimeEnvelope envelope)
    {
        var errors = RealtimeEnvelopeValidator.Validate(envelope);
        if (errors.Count != 0)
            throw new ArgumentException(string.Join(", ", errors), nameof(envelope));
        return JsonSerializer.Serialize(envelope, Options);
    }

    public static RealtimeEnvelope Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var envelope = JsonSerializer.Deserialize<RealtimeEnvelope>(json, Options);
        var errors = RealtimeEnvelopeValidator.Validate(envelope);
        if (errors.Count != 0)
            throw new JsonException(string.Join(", ", errors));
        return envelope!;
    }
}
