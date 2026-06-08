namespace CustomCodeFramework.Mongo.Collections;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MongoCollectionNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
