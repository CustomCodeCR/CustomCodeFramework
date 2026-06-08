using Npgsql;

namespace CustomCodeFramework.Postgres.Resilience;

public static class PostgresRetryPolicy
{
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        PostgresRetryOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(options);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await operation(cancellationToken);
                return;
            }
            catch (Exception exception)
                when (IsTransient(exception) && attempt <= options.MaxRetryCount)
            {
                var delay = CalculateDelay(attempt, options);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    public static async Task<TValue> ExecuteAsync<TValue>(
        Func<CancellationToken, Task<TValue>> operation,
        PostgresRetryOptions options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(options);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception exception)
                when (IsTransient(exception) && attempt <= options.MaxRetryCount)
            {
                var delay = CalculateDelay(attempt, options);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception exception)
    {
        return exception is TimeoutException || exception is NpgsqlException;
    }

    private static TimeSpan CalculateDelay(int attempt, PostgresRetryOptions options)
    {
        var delay = options.DelayMilliseconds * Math.Pow(2, attempt - 1);
        var boundedDelay = Math.Min(delay, options.MaxDelayMilliseconds);

        return TimeSpan.FromMilliseconds(boundedDelay);
    }
}
