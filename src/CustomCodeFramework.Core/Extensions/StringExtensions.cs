namespace CustomCodeFramework.Core.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string TrimOrEmpty(this string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    public static string? TrimOrNull(this string? value)
    {
        var trimmed = value?.Trim();

        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
