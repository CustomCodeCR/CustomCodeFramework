using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using CustomCodeFramework.Redis.Streams.Options;
using CustomCodeFramework.Redis.Streams.Serialization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.Streams.Publishing;

public sealed class RedisStreamPublisher(
    IConnectionMultiplexer connectionMultiplexer,
    RedisStreamMessageSerializer serializer,
    IOptions<RedisStreamOptions> streamOptions,
    IOptions<RedisStreamPublisherOptions> publisherOptions
) : IRedisStreamPublisher
{
    private readonly RedisStreamOptions _streamOptions = streamOptions.Value;
    private readonly RedisStreamPublisherOptions _publisherOptions = publisherOptions.Value;

    public async Task<string> PublishAsync(
        RedisStreamMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.MessageType);
        ArgumentNullException.ThrowIfNull(message.Payload);

        var database = connectionMultiplexer.GetDatabase();

        var streamName = string.IsNullOrWhiteSpace(message.StreamName)
            ? _streamOptions.DefaultStreamName
            : message.StreamName;

        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

        var payloadJson = serializer.Serialize(message.Payload);

        var headers = new Dictionary<string, string>(message.Headers)
        {
            [RedisStreamHeaders.MessageId] = Guid.NewGuid().ToString(),
            [RedisStreamHeaders.MessageType] = message.MessageType,
            [RedisStreamHeaders.SourceService] = _streamOptions.SourceService,
            [RedisStreamHeaders.CreatedAtUtc] = DateTime.UtcNow.ToString("O"),
        };

        var entries = new List<NameValueEntry>
        {
            new("message_type", message.MessageType),
            new("payload_json", payloadJson),
        };

        foreach (var header in headers)
        {
            entries.Add(new NameValueEntry($"header:{header.Key}", header.Value));
        }

        var id = await database.StreamAddAsync(
            streamName,
            entries.ToArray(),
            maxLength: _publisherOptions.MaxLength,
            useApproximateMaxLength: _publisherOptions.UseApproximateMaxLength
        );

        return id.ToString();
    }
}
