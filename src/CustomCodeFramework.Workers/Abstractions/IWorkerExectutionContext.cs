namespace CustomCodeFramework.Workers.Abstractions;

public interface IWorkerExecutionContext
{
    Guid ExecutionId { get; }

    string WorkerName { get; }

    DateTime StartedAtUtc { get; }

    IServiceProvider Services { get; }

    IReadOnlyDictionary<string, object?> Items { get; }
}
