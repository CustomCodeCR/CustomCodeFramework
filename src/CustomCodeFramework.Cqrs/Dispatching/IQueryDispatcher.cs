using CustomCodeFramework.Cqrs.Queries;

namespace CustomCodeFramework.Cqrs.Dispatching;

public interface IQueryDispatcher
{
    Task<TResponse> DispatchAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    );
}
