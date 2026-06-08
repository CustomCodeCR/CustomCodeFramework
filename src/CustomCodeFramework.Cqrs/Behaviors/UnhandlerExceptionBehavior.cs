using Microsoft.Extensions.Logging;

namespace CustomCodeFramework.Cqrs.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception for request {RequestName}",
                typeof(TRequest).Name
            );

            throw;
        }
    }
}
