namespace CustomCodeFramework.Reports.Storage;

public sealed record ReportStoragePath
{
    public required string Folder { get; init; }

    public required string FileName { get; init; }

    public string FullPath =>
        string.IsNullOrWhiteSpace(Folder) ? FileName : $"{Folder.TrimEnd('/')}/{FileName}";
}
