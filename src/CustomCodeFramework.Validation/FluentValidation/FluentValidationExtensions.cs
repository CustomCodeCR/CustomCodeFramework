using CustomCodeFramework.Validation.Errors;
using FluentValidation.Results;

namespace CustomCodeFramework.Validation.FluentValidation;

public static class FluentValidationExtensions
{
    public static IReadOnlyCollection<ValidationError> ToValidationErrors(
        this ValidationResult validationResult
    )
    {
        return validationResult
            .Errors.Select(failure => new ValidationError(
                failure.PropertyName,
                string.IsNullOrWhiteSpace(failure.ErrorCode)
                    ? ValidationErrorCodes.InvalidFormat
                    : failure.ErrorCode,
                failure.ErrorMessage,
                failure.AttemptedValue
            ))
            .ToArray();
    }
}
