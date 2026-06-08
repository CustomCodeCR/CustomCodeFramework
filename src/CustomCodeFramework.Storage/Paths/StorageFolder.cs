namespace CustomCodeFramework.Storage.Paths;

public sealed record StorageFolder
{
    public required string Value { get; init; }

    public string Normalized => Value.Trim().Trim('/').Replace("\\", "/", StringComparison.Ordinal);
}
