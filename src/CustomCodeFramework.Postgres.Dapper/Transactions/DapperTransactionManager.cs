using System.Data;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Persistence.Transactions;
using CustomCodeFramework.Postgres.Dapper.Abstractions;

namespace CustomCodeFramework.Postgres.Dapper.Transactions;

public sealed class DapperTransactionManager(ISqlConnectionFactory connectionFactory)
    : ITransactionManager
{
    public async Task<TransactionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        TransactionScopeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction(
            options?.IsolationLevel ?? IsolationLevel.ReadCommitted
        );

        try
        {
            await operation(cancellationToken);
            transaction.Commit();

            return TransactionResult.Committed();
        }
        catch (Exception exception)
        {
            transaction.Rollback();

            return TransactionResult.RolledBack(exception);
        }
    }

    public async Task<TransactionResult<TValue>> ExecuteAsync<TValue>(
        Func<CancellationToken, Task<TValue>> operation,
        TransactionScopeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction(
            options?.IsolationLevel ?? IsolationLevel.ReadCommitted
        );

        try
        {
            var value = await operation(cancellationToken);
            transaction.Commit();

            return TransactionResult<TValue>.Committed(value);
        }
        catch (Exception exception)
        {
            transaction.Rollback();

            return TransactionResult<TValue>.RolledBack(exception);
        }
    }
}
