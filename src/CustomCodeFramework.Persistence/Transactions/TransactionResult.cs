namespace CustomCodeFramework.Persistence.Transactions;

public class TransactionResult
{
    private TransactionResult(bool isCommitted, Exception? exception)
    {
        IsCommitted = isCommitted;
        Exception = exception;
    }

    public bool IsCommitted { get; }

    public bool IsRolledBack => !IsCommitted;

    public Exception? Exception { get; }

    public static TransactionResult Committed()
    {
        return new TransactionResult(true, null);
    }

    public static TransactionResult RolledBack(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new TransactionResult(false, exception);
    }
}

public sealed class TransactionResult<TValue>
{
    private TransactionResult(bool isCommitted, TValue? value, Exception? exception)
    {
        IsCommitted = isCommitted;
        Value = value;
        Exception = exception;
    }

    public bool IsCommitted { get; }

    public bool IsRolledBack => !IsCommitted;

    public TValue? Value { get; }

    public Exception? Exception { get; }

    public static TransactionResult<TValue> Committed(TValue value)
    {
        return new TransactionResult<TValue>(true, value, null);
    }

    public static TransactionResult<TValue> RolledBack(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new TransactionResult<TValue>(false, default, exception);
    }
}
