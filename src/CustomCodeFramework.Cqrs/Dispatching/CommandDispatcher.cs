using CustomCodeFramework.Cqrs.Behaviors;
using CustomCodeFramework.Cqrs.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Cqrs.Dispatching;

public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default
    )
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        RequestHandlerDelegate<Unit> handlerDelegate = async () =>
        {
            await handler.HandleAsync(command, cancellationToken);
            return Unit.Value;
        };

        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TCommand, Unit>>()
            .Reverse()
            .ToArray();

        var pipeline = behaviors.Aggregate(
            handlerDelegate,
            (next, behavior) => () => behavior.HandleAsync(command, next, cancellationToken)
        );

        await pipeline();
    }

    public async Task<TResponse> DispatchAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(
            commandType,
            typeof(TResponse)
        );

        dynamic handler = serviceProvider.GetRequiredService(handlerType);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            handler.HandleAsync((dynamic)command, cancellationToken);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(
            commandType,
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
            pipeline = () => behavior.HandleAsync((dynamic)command, next, cancellationToken);
        }

        return await pipeline();
    }
}

public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
