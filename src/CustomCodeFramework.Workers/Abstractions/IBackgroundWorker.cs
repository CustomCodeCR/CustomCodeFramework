namespace CustomCodeFramework.Workers.Abstractions;

public interface IBackgroundWorker
{
    string Name { get; }

    Task ExecuteAsync(IWorkerExecutionContext context, CancellationToken cancellationToken);
}
