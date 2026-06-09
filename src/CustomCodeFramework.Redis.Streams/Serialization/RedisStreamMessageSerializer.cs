using System.Text.Json;

namespace CustomCodeFramework.Redis.Streams.Serialization;

public sealed class RedisStreamMessageSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public T? Deserialize<T>(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}
