using System.Data;

namespace CustomCodeFramework.Persistence.Transactions;

public sealed record TransactionScopeOptions
{
    public IsolationLevel IsolationLevel { get; init; } = IsolationLevel.ReadCommitted;

    public TimeSpan? Timeout { get; init; }
}
