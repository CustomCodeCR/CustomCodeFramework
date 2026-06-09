using CustomCodeFramework.Messaging.Outbox.Locks;
using CustomCodeFramework.Messaging.Outbox.Options;
using CustomCodeFramework.Messaging.Outbox.Processing;
using CustomCodeFramework.Workers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Messaging.Outbox.BackgroundServices;

public sealed class OutboxBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<OutboxBackgroundServiceOptions> options,
    ILogger<OutboxBackgroundService> logger
) : BackgroundService
{
    private readonly OutboxBackgroundServiceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Outbox background service is disabled.");
            return;
        }

        if (_options.RunImmediately)
        {
            await ExecuteOnceSafelyAsync(stoppingToken);
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
            await ExecuteOnceAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogError(exception, "Outbox background service execution failed.");

            await Task.Delay(TimeSpan.FromSeconds(_options.ErrorDelaySeconds), cancellationToken);
        }
    }

    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        IAsyncDisposable? lockHandle = null;

        if (_options.UseDistributedLock)
        {
            var lockProvider = scope.ServiceProvider.GetService<IWorkerLockProvider>();

            if (lockProvider is not null)
            {
                lockHandle = await lockProvider.AcquireAsync(
                    OutboxWorkerLockKeys.OutboxProcessor,
                    TimeSpan.FromSeconds(_options.LockDurationSeconds),
                    cancellationToken
                );

                if (lockHandle is null)
                {
                    logger.LogDebug(
                        "Outbox background service skipped because lock was not acquired."
                    );

                    return;
                }
            }
        }

        await using (lockHandle)
        {
            var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

            OutboxProcessingResult result;

            do
            {
                result = await processor.ProcessAsync(_options.BatchSize, cancellationToken);

                logger.LogInformation(
                    "Outbox processed {ProcessedCount} message(s), failed {FailedCount} message(s). HasMoreMessages: {HasMoreMessages}.",
                    result.ProcessedCount,
                    result.FailedCount,
                    result.HasMoreMessages
                );
            } while (result.HasMoreMessages && !cancellationToken.IsCancellationRequested);
        }
    }
}
