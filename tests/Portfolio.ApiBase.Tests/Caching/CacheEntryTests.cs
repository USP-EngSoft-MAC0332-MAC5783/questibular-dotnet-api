namespace Portfolio.ApiBase.Tests.Caching;

public class CacheEntryTests
{
    [Fact]
    public void Constructor_ShouldSetValueAndCalculateExpiration()
    {
        // Arrange
        var lifetime = TimeSpan.FromMinutes(5);
        
        // Act
        var entry = new CacheEntry<string>("test_value", lifetime);
        
        // Assert
        entry.Value.Should().Be("test_value");
        entry.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(lifetime), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_ShouldChangeValueAndRenewTtl_WhenLifetimeProvided()
    {
        // Arrange
        var initialLifetime = TimeSpan.FromMinutes(1);
        var newLifetime = TimeSpan.FromMinutes(10);
        var entry = new CacheEntry<string>("old_value", initialLifetime);
        
        // Act
        entry.Update("new_value", newLifetime);
        
        // Assert
        entry.Value.Should().Be("new_value");
        entry.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(newLifetime), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_ShouldChangeValueAndKeepOriginalTtl_WhenLifetimeIsNull()
    {
        // Arrange
        var initialLifetime = TimeSpan.FromMinutes(10);
        var entry = new CacheEntry<string>("old_value", initialLifetime);
        var originalExpiration = entry.ExpiresAt;
        
        // Act
        entry.Update("new_value", null);
        
        // Assert
        entry.Value.Should().Be("new_value");
        // Borda: O TTL não deve ser alterado se newLifetime for null
        entry.ExpiresAt.Should().Be(originalExpiration); 
    }
}