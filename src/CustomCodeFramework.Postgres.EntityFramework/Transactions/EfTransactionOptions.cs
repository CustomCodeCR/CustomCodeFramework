using System.Data;

namespace CustomCodeFramework.Postgres.EntityFramework.Transactions;

public sealed record EfTransactionOptions
{
    public IsolationLevel IsolationLevel { get; init; } = IsolationLevel.ReadCommitted;
}
