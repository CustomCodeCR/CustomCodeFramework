using CustomCodeFramework.Cqrs.Commands;

namespace CustomCodeFramework.Cqrs.Dispatching;

public interface ICommandDispatcher
{
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<TResponse> DispatchAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    );
}
