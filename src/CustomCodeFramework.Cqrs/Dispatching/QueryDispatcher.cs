using CustomCodeFramework.Cqrs.Behaviors;
using CustomCodeFramework.Cqrs.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Cqrs.Dispatching;

public sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<TResponse> DispatchAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));

        dynamic handler = serviceProvider.GetRequiredService(handlerType);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            handler.HandleAsync((dynamic)query, cancellationToken);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(
            queryType,
            typeof(TResponse)
        );

        var behaviors = serviceProvider
            .GetServices(behaviorType)
            .Cast<dynamic>()
            .Reverse()
            .ToArray();

        RequestHandlerDelegate<TResponse> pipeline = handlerDelegate;

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => behavior.HandleAsync((dynamic)query, next, cancellationToken);
        }

        return await pipeline();
    }
}
