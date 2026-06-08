using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Persistence.Transactions;
using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.Transactions;

public sealed class EfTransactionManager(DbContext dbContext) : ITransactionManager
{
    public async Task<TransactionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        TransactionScopeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            options?.IsolationLevel ?? System.Data.IsolationLevel.ReadCommitted,
            cancellationToken
        );

        try
        {
            await operation(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return TransactionResult.Committed();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);

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

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            options?.IsolationLevel ?? System.Data.IsolationLevel.ReadCommitted,
            cancellationToken
        );

        try
        {
            var value = await operation(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return TransactionResult<TValue>.Committed(value);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return TransactionResult<TValue>.RolledBack(exception);
        }
    }
}
