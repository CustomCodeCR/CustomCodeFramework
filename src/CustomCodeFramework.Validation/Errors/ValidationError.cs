namespace CustomCodeFramework.Validation.Errors;

public sealed record ValidationError(
    string PropertyName,
    string Code,
    string Message,
    object? AttemptedValue = null
);
