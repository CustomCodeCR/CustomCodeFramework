namespace CustomCodeFramework.Reports.Excel.Styling;

public sealed class ExcelStyleOptions
{
    public bool BoldHeader { get; init; } = true;

    public bool CenterHeader { get; init; } = true;

    public bool ApplyBorders { get; init; } = true;

    public bool WrapText { get; init; } = false;
}
