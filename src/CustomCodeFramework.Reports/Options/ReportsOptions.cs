namespace CustomCodeFramework.Reports.Options;

public sealed class ReportsOptions
{
    public const string SectionName = "Reports";

    public string DefaultStorageFolder { get; init; } = "reports";

    public int DefaultExpirationDays { get; init; } = 30;

    public bool StoreGeneratedReports { get; init; } = false;

    public int MaxRows { get; init; } = 100_000;
}
