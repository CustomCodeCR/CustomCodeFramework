using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Persistence.Abstractions;
using CustomCodeFramework.Persistence.Transactions;

namespace CustomCodeFramework.Mongo.Transactions;

public sealed class MongoTransactionManager(IMongoContext mongoContext) : ITransactionManager
{
    public async Task<TransactionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        TransactionScopeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var session = await mongoContext.Client.StartSessionAsync(
            cancellationToken: cancellationToken
        );

        session.StartTransaction();

        try
        {
            await operation(cancellationToken);
            await session.CommitTransactionAsync(cancellationToken);

            return TransactionResult.Committed();
        }
        catch (Exception exception)
        {
            await session.AbortTransactionAsync(cancellationToken);

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

        using var session = await mongoContext.Client.StartSessionAsync(
            cancellationToken: cancellationToken
        );

        session.StartTransaction();

        try
        {
            var value = await operation(cancellationToken);
            await session.CommitTransactionAsync(cancellationToken);

            return TransactionResult<TValue>.Committed(value);
        }
        catch (Exception exception)
        {
            await session.AbortTransactionAsync(cancellationToken);

            return TransactionResult<TValue>.RolledBack(exception);
        }
    }
}
