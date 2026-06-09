using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Redis.Streams.Consuming;

public sealed class RedisStreamConsumerBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<RedisStreamConsumerOptions> options,
    ILogger<RedisStreamConsumerBackgroundService> logger
) : BackgroundService
{
    private readonly RedisStreamConsumerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Redis Stream consumer background service is disabled.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingIntervalSeconds));

        while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            await ExecuteOnceSafelyAsync(stoppingToken);
        }
    }

    private async Task ExecuteOnceSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();

            var consumer = scope.ServiceProvider.GetRequiredService<IRedisStreamConsumer>();

            var context = new RedisStreamConsumerContext
            {
                StreamName = _options.StreamName,
                ConsumerGroup = _options.ConsumerGroup,
                ConsumerName = _options.ConsumerName,
                BatchSize = _options.BatchSize,
                ReadPendingMessages = _options.ReadPendingMessages,
            };

            var result = await consumer.ConsumeAsync(context, cancellationToken);

            if (result.HasMessages)
            {
                logger.LogInformation(
                    "Redis Stream consumer processed {ProcessedCount} message(s), failed {FailedCount} message(s).",
                    result.ProcessedCount,
                    result.FailedCount
                );
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogError(exception, "Redis Stream consumer execution failed.");

            await Task.Delay(TimeSpan.FromSeconds(_options.ErrorDelaySeconds), cancellationToken);
        }
    }
}
