using CustomCodeFramework.Validation.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Api.EndpointFilters;

public sealed class ValidationEndpointFilter<TRequest>(IValidationService validationService)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return await next(context);
        }

        var result = await validationService.ValidateAsync(
            request,
            context.HttpContext.RequestAborted
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        return await next(context);
    }
}
