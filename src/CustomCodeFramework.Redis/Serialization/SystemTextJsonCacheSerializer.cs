using System.Text.Json;

namespace CustomCodeFramework.Redis.Serialization;

public sealed class SystemTextJsonCacheSerializer : ICacheSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public TValue? Deserialize<TValue>(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TValue>(value, JsonOptions);
    }
}
