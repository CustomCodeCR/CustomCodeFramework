namespace CustomCodeFramework.Reports.Excel.Styling;

public sealed record ExcelColumnStyle
{
    public required string ColumnKey { get; init; }

    public string? NumberFormat { get; init; }

    public double? Width { get; init; }

    public bool IsBold { get; init; }

    public bool WrapText { get; init; }
}
