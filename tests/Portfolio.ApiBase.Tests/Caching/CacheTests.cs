namespace Portfolio.ApiBase.Tests.Caching;

public class CacheTests
{
    [Fact]
    public void CacheEntry_Should_Be_Expired_If_Past_Lifetime()
    {
        // Arrange
        var entry = CacheEntry<string>.Create("value", TimeSpan.FromMilliseconds(-10));

        // Act
        var isExpired = entry.IsExpired;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void CacheEntry_Should_Not_Be_Expired_If_No_Lifetime()
    {
        // Arrange
        var entry = CacheEntry<string>.Create("value", null);

        // Act
        var isExpired = entry.IsExpired;

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void CacheEntry_Update_Should_Change_Value_And_Timestamp()
    {
        // Arrange
        var entry = CacheEntry<string>.Create("old");

        // Act
        entry.Update("new", TimeSpan.FromMinutes(5));

        // Assert
        entry.Value.Should().Be("new");
        entry.UpdatedAt.Should().NotBeNull();
        entry.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrCreateWithMetadataAsync_Should_Call_Factory_When_Cache_Is_Empty()
    {
        // Arrange
        var cache = new FakeCacheService();
        var factoryCalled = false;

        Task<string> Factory(CancellationToken ct)
        {
            factoryCalled = true;
            return Task.FromResult("factory-value");
        }

        // Act
        var result = await cache.GetOrCreateWithMetadataAsync("key", Factory);

        // Assert
        result.Should().Be("factory-value");
        factoryCalled.Should().BeTrue();
        (await cache.ExistsAsync("key")).Should().BeTrue();
    }

    [Fact]
    public async Task GetOrCreateWithMetadataAsync_Should_Return_Cached_Value_If_Not_Expired()
    {
        // Arrange
        var cache = new FakeCacheService();
        await cache.SetAsync("key", CacheEntry<string>.Create("cached-value", TimeSpan.FromMinutes(10)), TimeSpan.FromMinutes(10));
        var factoryCalled = false;

        // Act
        var result = await cache.GetOrCreateWithMetadataAsync("key", _ => { factoryCalled = true; return Task.FromResult("factory"); });

        // Assert
        result.Should().Be("cached-value");
        factoryCalled.Should().BeFalse(); // Factory não deve ser chamada
    }

    private class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _store = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.TryGetValue(key, out var val) ? (T)val : default);

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            _store[key] = value!;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) { _store.Remove(key); return Task.CompletedTask; }
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(_store.ContainsKey(key));
        public Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}