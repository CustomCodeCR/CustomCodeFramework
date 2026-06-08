using Microsoft.Extensions.Logging;

namespace CustomCodeFramework.Cqrs.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling request {RequestName}", requestName);

        var response = await next();

        logger.LogInformation("Handled request {RequestName}", requestName);

        return response;
    }
}
