namespace CustomCodeFramework.Redis.Streams.Messages;

public static class RedisStreamHeaders
{
    public const string MessageId = "message_id";

    public const string MessageType = "message_type";

    public const string CorrelationId = "correlation_id";

    public const string CausationId = "causation_id";

    public const string SourceService = "source_service";

    public const string CreatedAtUtc = "created_at_utc";
}
