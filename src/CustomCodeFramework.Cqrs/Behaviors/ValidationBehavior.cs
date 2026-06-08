using CustomCodeFramework.Validation.Abstractions;
using CustomCodeFramework.Validation.Exceptions;

namespace CustomCodeFramework.Cqrs.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IValidationService validationService)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        var result = await validationService.ValidateAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            throw new ValidationException([]);
        }

        return await next();
    }
}
