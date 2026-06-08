using CustomCodeFramework.Validation.Errors;

namespace CustomCodeFramework.Validation.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyCollection<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }
}
