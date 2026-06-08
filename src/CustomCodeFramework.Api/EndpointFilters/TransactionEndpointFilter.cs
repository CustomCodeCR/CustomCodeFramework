using CustomCodeFramework.Persistence.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Api.EndpointFilters;

public sealed class TransactionEndpointFilter(ITransactionManager transactionManager)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var result = await transactionManager.ExecuteAsync<object?>(
            async cancellationToken =>
            {
                return await next(context);
            },
            cancellationToken: context.HttpContext.RequestAborted
        );

        if (result.IsRolledBack)
        {
            throw result.Exception
                ?? new InvalidOperationException("The transaction was rolled back.");
        }

        return result.Value;
    }
}
