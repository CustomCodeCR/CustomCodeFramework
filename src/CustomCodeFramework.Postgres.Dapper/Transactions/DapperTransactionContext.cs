using System.Data;

namespace CustomCodeFramework.Postgres.Dapper.Transactions;

public sealed class DapperTransactionContext(IDbConnection connection, IDbTransaction transaction)
    : IDisposable
{
    public IDbConnection Connection { get; } = connection;

    public IDbTransaction Transaction { get; } = transaction;

    public void Dispose()
    {
        Transaction.Dispose();
        Connection.Dispose();
    }
}
