using System.Globalization;

namespace CustomCodeFramework.Reports.Csv.Formatting;

public static class CsvColumnFormatter
{
    public static object? FormatValue(
        object? value,
        string? format = null,
        IFormatProvider? formatProvider = null
    )
    {
        if (value is null)
        {
            return null;
        }

        formatProvider ??= CultureInfo.InvariantCulture;

        if (string.IsNullOrWhiteSpace(format))
        {
            return value;
        }

        return value switch
        {
            DateTime dateTime => dateTime.ToString(format, formatProvider),
            DateOnly dateOnly => dateOnly.ToString(format, formatProvider),
            TimeOnly timeOnly => timeOnly.ToString(format, formatProvider),
            decimal decimalValue => decimalValue.ToString(format, formatProvider),
            double doubleValue => doubleValue.ToString(format, formatProvider),
            float floatValue => floatValue.ToString(format, formatProvider),
            int intValue => intValue.ToString(format, formatProvider),
            long longValue => longValue.ToString(format, formatProvider),
            _ => value,
        };
    }
}
