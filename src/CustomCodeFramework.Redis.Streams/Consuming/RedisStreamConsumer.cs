using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using StackExchange.Redis;

namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed class RedisStreamConsumer(
    IConnectionMultiplexer connectionMultiplexer,
    IEnumerable<IRedisStreamMessageHandler> handlers
) : IRedisStreamConsumer
{
    public async Task<RedisStreamConsumerResult> ConsumeAsync(
        RedisStreamConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.StreamName);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.ConsumerGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.ConsumerName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(context.BatchSize, 0);

        var database = connectionMultiplexer.GetDatabase();

        await EnsureConsumerGroupAsync(database, context.StreamName, context.ConsumerGroup);

        var entries = await database.StreamReadGroupAsync(
            context.StreamName,
            context.ConsumerGroup,
            context.ConsumerName,
            ">",
            context.BatchSize
        );

        if (entries.Length == 0)
        {
            return RedisStreamConsumerResult.Empty;
        }

        var processed = 0;
        var failed = 0;

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var envelope = MapEnvelope(context.StreamName, entry);

            var handler = handlers.FirstOrDefault(x =>
                x.MessageType.Equals(envelope.MessageType, StringComparison.OrdinalIgnoreCase)
            );

            if (handler is null)
            {
                failed++;
                continue;
            }

            try
            {
                await handler.HandleAsync(envelope, cancellationToken);

                await database.StreamAcknowledgeAsync(
                    context.StreamName,
                    context.ConsumerGroup,
                    entry.Id
                );

                processed++;
            }
            catch
            {
                failed++;
            }
        }

        return new RedisStreamConsumerResult(processed, failed, HasMessages: true);
    }

    private static async Task EnsureConsumerGroupAsync(
        IDatabase database,
        string streamName,
        string consumerGroup
    )
    {
        try
        {
            await database.StreamCreateConsumerGroupAsync(
                streamName,
                consumerGroup,
                StreamPosition.NewMessages,
                createStream: true
            );
        }
        catch (RedisServerException exception)
            when (exception.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase)) { }
    }

    private static RedisStreamEnvelope MapEnvelope(string streamName, StreamEntry entry)
    {
        var values = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        var headers = values
            .Where(x => x.Key.StartsWith("header:", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key["header:".Length..], x => x.Value);

        return new RedisStreamEnvelope
        {
            StreamName = streamName,
            MessageId = entry.Id.ToString(),
            MessageType = values.GetValueOrDefault("message_type") ?? string.Empty,
            PayloadJson = values.GetValueOrDefault("payload_json") ?? string.Empty,
            Headers = headers,
            ReceivedAtUtc = DateTime.UtcNow,
        };
    }
}
