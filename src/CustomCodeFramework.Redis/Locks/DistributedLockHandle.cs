using StackExchange.Redis;

namespace CustomCodeFramework.Redis.Locks;

public sealed class DistributedLockHandle(IDatabase database, string key, RedisValue value)
    : IAsyncDisposable
{
    public string Key { get; } = key;

    public RedisValue Value { get; } = value;

    public async ValueTask DisposeAsync()
    {
        const string script = """
            if redis.call("get", KEYS[1]) == ARGV[1] then
                return redis.call("del", KEYS[1])
            else
                return 0
            end
            """;

        await database.ScriptEvaluateAsync(script, [Key], [Value]);
    }
}
