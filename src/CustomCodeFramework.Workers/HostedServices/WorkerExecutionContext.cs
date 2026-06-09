using CustomCodeFramework.Workers.Abstractions;

namespace CustomCodeFramework.Workers.HostedServices;

internal sealed class WorkerExecutionContext : IWorkerExecutionContext
{
    private readonly Dictionary<string, object?> _items = [];

    public WorkerExecutionContext(string workerName, IServiceProvider services)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerName);
        ArgumentNullException.ThrowIfNull(services);

        ExecutionId = Guid.NewGuid();
        WorkerName = workerName;
        StartedAtUtc = DateTime.UtcNow;
        Services = services;
    }

    public Guid ExecutionId { get; }

    public string WorkerName { get; }

    public DateTime StartedAtUtc { get; }

    public IServiceProvider Services { get; }

    public IReadOnlyDictionary<string, object?> Items => _items;

    public void SetItem(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _items[key] = value;
    }
}
