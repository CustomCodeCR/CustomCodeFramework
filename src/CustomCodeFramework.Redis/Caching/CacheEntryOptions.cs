namespace CustomCodeFramework.Redis.Caching;

public sealed record CacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; init; }

    public TimeSpan? SlidingExpiration { get; init; }

    public static CacheEntryOptions Default(TimeSpan expiration)
    {
        return new CacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
    }
}
