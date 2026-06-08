using CustomCodeFramework.Validation.Errors;

namespace CustomCodeFramework.Validation.Abstractions;

public interface IValidationRule<in TRequest>
{
    Task<IReadOnlyCollection<ValidationError>> ValidateAsync(
        TRequest request,
        CancellationToken cancellationToken = default
    );
}
