using System.Text.Json;
using CustomCodeFramework.Messaging.Realtime;
using Xunit;

namespace CustomCodeFramework.Messaging.Realtime.Tests;

public sealed class RealtimeContractTests
{
    private static JsonElement Payload(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static RealtimeEnvelope Valid() => new()
    {
        EventId = Guid.Parse("55555555-5555-4555-8555-555555555555"),
        EventType = "crm.account.updated.v1",
        SchemaVersion = 1,
        AggregateType = "account",
        AggregateId = "acc-123",
        AggregateVersion = 1,
        OccurredAtUtc = new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero),
        Producer = "DholeCRMService",
        CorrelationId = "corr-123",
        AudienceRef = "crm:account:acc-123",
        Payload = Payload("""{"changed":true,"revision":2}"""),
        TraceId = "trace-123"
    };

    [Fact]
    public void ValidMinimalEventHasNoErrors() =>
        Assert.Empty(RealtimeEnvelopeValidator.Validate(Valid()));

    [Fact]
    public void MissingEventIdIsRejected() =>
        Assert.Contains("eventId.required", RealtimeEnvelopeValidator.Validate(Valid() with { EventId = Guid.Empty }));

    [Fact]
    public void InvalidEventTypeIsRejected() =>
        Assert.Contains("eventType.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { EventType = "CRM Account Updated" }));

    [Fact]
    public void ZeroSchemaVersionIsRejected() =>
        Assert.Contains("schemaVersion.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { SchemaVersion = 0 }));

    [Fact]
    public void EmptyAggregateTypeIsRejected() =>
        Assert.Contains("aggregateType.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { AggregateType = "" }));

    [Fact]
    public void EmptyAggregateIdIsRejected() =>
        Assert.Contains("aggregateId.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { AggregateId = "" }));

    [Fact]
    public void ZeroAggregateVersionIsRejected() =>
        Assert.Contains("aggregateVersion.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { AggregateVersion = 0 }));

    [Fact]
    public void NonUtcTimestampIsRejected() =>
        Assert.Contains("occurredAtUtc.invalid", RealtimeEnvelopeValidator.Validate(Valid() with {
            OccurredAtUtc = new DateTimeOffset(2026, 11, 7, 6, 0, 0, TimeSpan.FromHours(-6))
        }));

    [Fact]
    public void DefaultTimestampIsRejected() =>
        Assert.Contains("occurredAtUtc.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { OccurredAtUtc = default }));

    [Fact]
    public void EmptyProducerIsRejected() =>
        Assert.Contains("producer.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { Producer = "" }));

    [Fact]
    public void EmptyCorrelationIsRejected() =>
        Assert.Contains("correlationId.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { CorrelationId = "" }));

    [Fact]
    public void EmptyAudienceReferenceIsRejected() =>
        Assert.Contains("audienceRef.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { AudienceRef = "" }));

    [Fact]
    public void OversizedTraceIdIsRejected() =>
        Assert.Contains("traceId.invalid", RealtimeEnvelopeValidator.Validate(Valid() with { TraceId = new string('x', 161) }));

    [Fact]
    public void PayloadMustBeAnObject() =>
        Assert.Contains("payload.objectRequired", RealtimeEnvelopeValidator.Validate(Valid() with { Payload = Payload("""[1,2,3]""") }));

    [Fact]
    public void NestedSecretIsRejected() =>
        Assert.Contains("payload.forbiddenField:password", RealtimeEnvelopeValidator.Validate(Valid() with {
            Payload = Payload("""{"meta":{"password":"secret"}}""")
        }));

    [Fact]
    public void ArrayNestedPersonalDataIsRejected() =>
        Assert.Contains("payload.forbiddenField:email", RealtimeEnvelopeValidator.Validate(Valid() with {
            Payload = Payload("""{"changes":[{"email":"example@example.com"}]}""")
        }));

    [Fact]
    public void SecretKeyNamesAreCaseInsensitive() =>
        Assert.Contains("payload.forbiddenField:ACCESSTOKEN", RealtimeEnvelopeValidator.Validate(Valid() with {
            Payload = Payload("""{"ACCESSTOKEN":"sensitive"}""")
        }));

    [Fact]
    public void PayloadLargerThanFourKiBIsRejected() =>
        Assert.Contains("payload.tooLarge", RealtimeEnvelopeValidator.Validate(Valid() with {
            Payload = Payload("{\"details\":\"" + new string('x', 4200) + "\"}")
        }));

    [Fact]
    public void SafeMinimalInvalidationPayloadIsAllowed() =>
        Assert.Empty(RealtimeEnvelopeValidator.Validate(Valid() with {
            Payload = Payload("""{"changedFields":["status"],"aggregateVersion":2}""")
        }));

    [Fact]
    public void NullEnvelopeIsRejected() =>
        Assert.Contains("envelope.required", RealtimeEnvelopeValidator.Validate(null));

    [Fact]
    public void SerializationUsesCamelCase() =>
        Assert.Contains("\"eventId\"", RealtimeEnvelopeCodec.Serialize(Valid()));

    [Fact]
    public void RoundTripKeepsCorrelationAndVersion()
    {
        var got = RealtimeEnvelopeCodec.Deserialize(RealtimeEnvelopeCodec.Serialize(Valid()));
        Assert.Equal(Valid().EventId, got.EventId);
        Assert.Equal(1, got.SchemaVersion);
        Assert.Equal("corr-123", got.CorrelationId);
        Assert.Equal(JsonValueKind.Object, got.Payload.ValueKind);
    }

    [Fact]
    public void SerializationRejectsSensitivePayload() =>
        Assert.Throws<ArgumentException>(() => RealtimeEnvelopeCodec.Serialize(Valid() with {
            Payload = Payload("""{"refreshToken":"x"}""")
        }));

    [Fact]
    public void DeserializationRejectsInvalidEvent() =>
        Assert.Throws<JsonException>(() => RealtimeEnvelopeCodec.Deserialize(
            RealtimeEnvelopeCodec.Serialize(Valid()).Replace(
                "crm.account.updated.v1", "INVALID EVENT")));

    [Fact]
    public void DeserializationRejectsMissingJson() =>
        Assert.Throws<ArgumentException>(() => RealtimeEnvelopeCodec.Deserialize(""));

    [Theory]
    [InlineData("crm.account.updated.v1", true)]
    [InlineData("pricing.quote.approved.v2", true)]
    [InlineData("crm.updated.v1", false)]
    [InlineData("CRM.account.updated.v1", false)]
    [InlineData("crm.account.updated", false)]
    [InlineData("crm.account.updated.v0", false)]
    [InlineData("", false)]
    public void EventTypeNamingRule(string value, bool expected) =>
        Assert.Equal(expected, RealtimeEventType.IsValid(value));

    [Fact]
    public void SchemaConsumerAcceptsMatchingVersion() =>
        Assert.True(RealtimeSchemaEvolution.CanConsume("crm.account.updated.v1", 1, Valid()));

    [Fact]
    public void SchemaConsumerRejectsBreakingEventType() =>
        Assert.False(RealtimeSchemaEvolution.CanConsume("crm.account.updated.v2", 1, Valid()));

    [Fact]
    public void SchemaConsumerRejectsInsufficientSchemaVersion() =>
        Assert.False(RealtimeSchemaEvolution.CanConsume("crm.account.updated.v1", 2, Valid()));

    [Fact]
    public void SchemaConsumerRejectsZeroMinimum() =>
        Assert.False(RealtimeSchemaEvolution.CanConsume("crm.account.updated.v1", 0, Valid()));

    [Fact]
    public void DuplicateAggregateVersionIsStale() =>
        Assert.Equal(RealtimeVersionDecision.StaleOrDuplicate, RealtimeSchemaEvolution.CompareVersions(4, 4));

    [Fact]
    public void OutOfOrderLowerVersionIsStale() =>
        Assert.Equal(RealtimeVersionDecision.StaleOrDuplicate, RealtimeSchemaEvolution.CompareVersions(4, 3));

    [Fact]
    public void NextAggregateVersionCanBeApplied() =>
        Assert.Equal(RealtimeVersionDecision.Next, RealtimeSchemaEvolution.CompareVersions(4, 5));

    [Fact]
    public void VersionGapRequiresRestReconciliation() =>
        Assert.Equal(RealtimeVersionDecision.GapRequiresRestReconciliation, RealtimeSchemaEvolution.CompareVersions(4, 7));

    [Fact]
    public void InvalidAggregateVersionIsRejected() =>
        Assert.Equal(RealtimeVersionDecision.Invalid, RealtimeSchemaEvolution.CompareVersions(4, 0));

    [Fact]
    public void NegativeCurrentVersionIsRejected() =>
        Assert.Equal(RealtimeVersionDecision.Invalid, RealtimeSchemaEvolution.CompareVersions(-1, 1));
}
