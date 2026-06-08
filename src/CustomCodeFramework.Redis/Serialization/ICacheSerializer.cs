namespace CustomCodeFramework.Redis.Serialization;

public interface ICacheSerializer
{
    string Serialize<TValue>(TValue value);

    TValue? Deserialize<TValue>(string value);
}
