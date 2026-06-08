namespace CustomCodeFramework.Validation.Errors;

public static class ValidationErrorCodes
{
    public const string Required = "validation.required";
    public const string InvalidFormat = "validation.invalid_format";
    public const string InvalidEmail = "validation.invalid_email";
    public const string InvalidGuid = "validation.invalid_guid";
    public const string MaxLength = "validation.max_length";
    public const string MinLength = "validation.min_length";
    public const string GreaterThan = "validation.greater_than";
    public const string GreaterThanOrEqual = "validation.greater_than_or_equal";
    public const string LessThan = "validation.less_than";
    public const string LessThanOrEqual = "validation.less_than_or_equal";
    public const string DateRange = "validation.date_range";
    public const string InvalidEnum = "validation.invalid_enum";
}
