namespace CustomCodeFramework.Redis.Abstractions;

public interface ICacheKeyBuilder
{
    string Build(params string[] parts);
}
