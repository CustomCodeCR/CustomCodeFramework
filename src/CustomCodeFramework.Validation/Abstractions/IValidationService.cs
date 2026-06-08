using CustomCodeFramework.Core.Results;

namespace CustomCodeFramework.Validation.Abstractions;

public interface IValidationService
{
    Task<Result> ValidateAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default
    );
}
