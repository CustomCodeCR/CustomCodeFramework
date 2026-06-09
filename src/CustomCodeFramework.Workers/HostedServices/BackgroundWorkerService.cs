using CustomCodeFramework.Workers.Abstractions;
using CustomCodeFramework.Workers.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Workers.HostedServices;

public sealed class BackgroundWorkerService<TWorker>(
    IServiceProvider serviceProvider,
    IOptions<WorkerOptions> options,
    ILogger<BackgroundWorkerService<TWorker>> logger
) : BackgroundService
    where TWorker : class, IBackgroundWorker
{
    private readonly WorkerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Worker {WorkerType} is disabled.", typeof(TWorker).Name);

            return;
        }

        using var scope = serviceProvider.CreateScope();

        var worker = scope.ServiceProvider.GetRequiredService<TWorker>();

        var context = new WorkerExecutionContext(worker.Name, scope.ServiceProvider);

        logger.LogInformation(
            "Starting worker {WorkerName} with execution {ExecutionId}.",
            context.WorkerName,
            context.ExecutionId
        );

        await worker.ExecuteAsync(context, stoppingToken);

        logger.LogInformation(
            "Worker {WorkerName} finished execution {ExecutionId}.",
            context.WorkerName,
            context.ExecutionId
        );
    }
}
