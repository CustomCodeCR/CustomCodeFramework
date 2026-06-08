using System.Text.Json;

namespace CustomCodeFramework.Messaging.Serialization;

public sealed class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public TValue? Deserialize<TValue>(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return JsonSerializer.Deserialize<TValue>(payload, JsonOptions);
    }

    public object? Deserialize(string payload, Type type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentNullException.ThrowIfNull(type);

        return JsonSerializer.Deserialize(payload, type, JsonOptions);
    }
}
