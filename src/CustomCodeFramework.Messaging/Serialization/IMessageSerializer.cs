namespace CustomCodeFramework.Messaging.Serialization;

public interface IMessageSerializer
{
    string Serialize<TValue>(TValue value);

    TValue? Deserialize<TValue>(string payload);

    object? Deserialize(string payload, Type type);
}
