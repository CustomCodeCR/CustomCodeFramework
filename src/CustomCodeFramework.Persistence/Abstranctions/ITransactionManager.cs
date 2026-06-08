using CustomCodeFramework.Persistence.Transactions;

namespace CustomCodeFramework.Persistence.Abstractions;

public interface ITransactionManager
{
    Task<TransactionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        TransactionScopeOptions? options = null,
        CancellationToken cancellationToken = default
    );

    Task<TransactionResult<TValue>> ExecuteAsync<TValue>(
        Func<CancellationToken, Task<TValue>> operation,
        TransactionScopeOptions? options = null,
        CancellationToken cancellationToken = default
    );
}
