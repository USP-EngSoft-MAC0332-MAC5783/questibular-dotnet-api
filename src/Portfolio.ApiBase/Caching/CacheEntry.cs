namespace Portfolio.ApiBase.Caching;

/// <summary>
/// Wraps a cached value with metadata such as creation time and absolute expiration.
/// </summary>
public class CacheEntry<T>
{
    [JsonConstructor]
    public CacheEntry(T value, TimeSpan? lifetime)
    {
        Value = value;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = lifetime.HasValue ? DateTime.UtcNow.Add(lifetime.Value) : null;
    }

    /// <summary>
    /// The actual cached data.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// When this item was first stored in the cache.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When this item was last updated (if modified).
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// When this item will expire. Null means never expires.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Computed property indicating if the item has expired.
    /// </summary>
    [JsonIgnore]
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Creates a new CacheEntry with the current UTC time as CreatedAt.
    /// </summary>
    public static CacheEntry<T> Create(T value, TimeSpan? lifetime = null)
        => new(value, lifetime);

    /// <summary>
    /// Updates the stored value and refreshes the UpdatedAt timestamp.
    /// Optionally renews the expiration time.
    /// </summary>
    public void Update(T newValue, TimeSpan? newLifetime = null)
    {
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;

        if (newLifetime.HasValue)
            ExpiresAt = DateTime.UtcNow.Add(newLifetime.Value);
    }

    /// <summary>
    /// Refreshes the expiration time (touch the entry).
    /// </summary>
    public void Touch(TimeSpan newLifetime)
    {
        ExpiresAt = DateTime.UtcNow.Add(newLifetime);
        UpdatedAt = DateTime.UtcNow;
    }
}