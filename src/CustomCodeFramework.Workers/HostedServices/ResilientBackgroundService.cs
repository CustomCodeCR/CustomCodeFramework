using CustomCodeFramework.Workers.Abstractions;
using CustomCodeFramework.Workers.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Workers.HostedServices;

public sealed class ResilientBackgroundService<TWorker>(
    IServiceProvider serviceProvider,
    IOptions<WorkerOptions> options,
    ILogger<ResilientBackgroundService<TWorker>> logger
) : BackgroundService
    where TWorker : class, IBackgroundWorker
{
    private readonly WorkerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation(
                "Resilient worker {WorkerType} is disabled.",
                typeof(TWorker).Name
            );

            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var retryCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteWorkerAsync(stoppingToken);
                    break;
                }
                catch (Exception exception)
                {
                    retryCount++;

                    logger.LogError(
                        exception,
                        "Worker {WorkerType} failed. Retry {RetryCount}/{MaxRetryCount}.",
                        typeof(TWorker).Name,
                        retryCount,
                        _options.MaxRetryCount
                    );

                    if (_options.StopOnUnhandledException || retryCount >= _options.MaxRetryCount)
                    {
                        throw;
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(_options.RetryDelaySeconds),
                        stoppingToken
                    );
                }
            }
        }
    }

    private async Task ExecuteWorkerAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var worker = scope.ServiceProvider.GetRequiredService<TWorker>();

        var context = new WorkerExecutionContext(worker.Name, scope.ServiceProvider);

        IAsyncDisposable? lockHandle = null;

        if (_options.UseDistributedLock)
        {
            var lockProvider = scope.ServiceProvider.GetService<IWorkerLockProvider>();

            if (lockProvider is not null)
            {
                lockHandle = await lockProvider.AcquireAsync(
                    $"workers:{worker.Name}",
                    TimeSpan.FromSeconds(_options.LockDurationSeconds),
                    cancellationToken
                );

                if (lockHandle is null)
                {
                    return;
                }
            }
        }

        await using (lockHandle)
        {
            await worker.ExecuteAsync(context, cancellationToken);
        }
    }
}
