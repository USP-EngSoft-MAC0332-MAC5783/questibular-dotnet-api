namespace Portfolio.ApiBase.Caching;

public static class CacheExtensions
{
    /// <summary>
    /// Extension to wrap cache items with <see cref="CacheEntry{T}"/> to leverage metadata like expiration checks.
    /// </summary>
    public static async Task<T?> GetOrCreateWithMetadataAsync<T>(
        this ICacheService cache,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? lifetime = null,
        CancellationToken cancellationToken = default)
    {
        var entry = await cache.GetAsync<CacheEntry<T>>(key, cancellationToken);
        
        if (entry != null && !entry.IsExpired)
            return entry.Value;

        var value = await factory(cancellationToken);
        var newEntry = CacheEntry<T>.Create(value, lifetime);
        await cache.SetAsync(key, newEntry, lifetime, cancellationToken);
        
        return value;
    }
}