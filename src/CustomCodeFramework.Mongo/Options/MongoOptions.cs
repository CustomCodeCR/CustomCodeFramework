namespace CustomCodeFramework.Mongo.Options;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; init; } = string.Empty;

    public string DatabaseName { get; init; } = string.Empty;

    public bool EnableHealthCheck { get; init; } = true;
}
