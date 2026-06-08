using CustomCodeFramework.Validation.Errors;
using FluentValidation;

namespace CustomCodeFramework.Validation.FluentValidation;

public static class CommonRules
{
    public static IRuleBuilderOptions<T, string> RequiredString<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder.NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }

    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder.EmailAddress().WithErrorCode(ValidationErrorCodes.InvalidEmail);
    }

    public static IRuleBuilderOptions<T, Guid> NotEmptyGuid<T>(
        this IRuleBuilder<T, Guid> ruleBuilder
    )
    {
        return ruleBuilder.NotEmpty().WithErrorCode(ValidationErrorCodes.InvalidGuid);
    }

    public static IRuleBuilderOptions<T, Guid?> NotEmptyGuid<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder
    )
    {
        return ruleBuilder.NotEmpty().WithErrorCode(ValidationErrorCodes.InvalidGuid);
    }

    public static IRuleBuilderOptions<T, decimal> PositiveAmount<T>(
        this IRuleBuilder<T, decimal> ruleBuilder
    )
    {
        return ruleBuilder.GreaterThan(0).WithErrorCode(ValidationErrorCodes.GreaterThan);
    }

    public static IRuleBuilderOptions<T, int> PositiveNumber<T>(
        this IRuleBuilder<T, int> ruleBuilder
    )
    {
        return ruleBuilder.GreaterThan(0).WithErrorCode(ValidationErrorCodes.GreaterThan);
    }

    public static IRuleBuilderOptions<T, string?> MaxLengthSafe<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength
    )
    {
        return ruleBuilder.MaximumLength(maxLength).WithErrorCode(ValidationErrorCodes.MaxLength);
    }
}
