using CustomCodeFramework.Workers.Abstractions;
using CustomCodeFramework.Workers.Options;
using CustomCodeFramework.Workers.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Workers.HostedServices;

public sealed class PeriodicBackgroundService<TWorker>(
    IServiceProvider serviceProvider,
    IOptions<WorkerOptions> workerOptions,
    IOptions<WorkerScheduleOptions> scheduleOptions,
    ILogger<PeriodicBackgroundService<TWorker>> logger
) : BackgroundService
    where TWorker : class, IBackgroundWorker
{
    private readonly WorkerOptions _workerOptions = workerOptions.Value;
    private readonly WorkerScheduleOptions _scheduleOptions = scheduleOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_workerOptions.Enabled)
        {
            logger.LogInformation(
                "Periodic worker {WorkerType} is disabled.",
                typeof(TWorker).Name
            );

            return;
        }

        if (_scheduleOptions.RunImmediately)
        {
            await ExecuteWorkerAsync(stoppingToken);
        }

        await using var timer = new WorkerTimer(_scheduleOptions.Interval);

        while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            await ExecuteWorkerAsync(stoppingToken);
        }
    }

    private async Task ExecuteWorkerAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var worker = scope.ServiceProvider.GetRequiredService<TWorker>();

        var context = new WorkerExecutionContext(worker.Name, scope.ServiceProvider);

        logger.LogInformation(
            "Executing periodic worker {WorkerName} with execution {ExecutionId}.",
            context.WorkerName,
            context.ExecutionId
        );

        await worker.ExecuteAsync(context, cancellationToken);
    }
}
