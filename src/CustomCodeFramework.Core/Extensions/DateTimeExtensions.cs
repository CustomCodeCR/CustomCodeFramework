namespace CustomCodeFramework.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToUtcUnspecifiedAsUtc(this DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    public static bool IsBetween(
        this DateTime value,
        DateTime start,
        DateTime end,
        bool inclusive = true
    )
    {
        return inclusive ? value >= start && value <= end : value > start && value < end;
    }
}
