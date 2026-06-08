namespace CustomCodeFramework.Core.Guards;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        return value;
    }

    public static T AgainstNull<T>(T? value, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        return value;
    }

    public static Guid AgainstEmptyGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("The value cannot be an empty GUID.", parameterName);
        }

        return value;
    }

    public static int AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value must be greater than zero."
            );
        }

        return value;
    }

    public static decimal AgainstNegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value must be greater than zero."
            );
        }

        return value;
    }
}
