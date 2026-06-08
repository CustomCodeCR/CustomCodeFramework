using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Options;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Redis.Caching;

public sealed class CacheKeyBuilder(IOptions<RedisOptions> options) : ICacheKeyBuilder
{
    public string Build(params string[] parts)
    {
        var validParts = parts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim().ToLowerInvariant());

        var key = string.Join(":", validParts);

        return $"{options.Value.InstanceName}{key}";
    }
}
