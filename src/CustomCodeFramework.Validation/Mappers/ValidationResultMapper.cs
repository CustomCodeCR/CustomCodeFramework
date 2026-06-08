using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Validation.Errors;

namespace CustomCodeFramework.Validation.Mappers;

public static class ValidationResultMapper
{
    public static Error ToError(IReadOnlyCollection<ValidationError> errors)
    {
        if (errors.Count == 0)
        {
            return Error.None;
        }

        var message = string.Join(
            "; ",
            errors.Select(error => $"{error.PropertyName}: {error.Message}")
        );

        return Error.Create("validation.failed", message);
    }
}
