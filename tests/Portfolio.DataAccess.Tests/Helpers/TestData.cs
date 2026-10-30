namespace Portfolio.DataAccess.Tests.Helpers;

/// <summary>
/// Factory helpers for building valid domain aggregates.
/// Kept explicit so each test only overrides the fields it cares about
/// — random defaults hide nothing and make failures easier to reproduce.
/// </summary>
internal static class TestData
{
    public static User CreateUser(string? email = null, DateTime? now = null)
        => User.Create(
            email ?? $"user-{Guid.NewGuid():N}@example.com",
            passwordHash: "$2a$11$not-a-real-hash",
            now: now ?? DateTime.UtcNow);

    public static RefreshToken CreateRefreshToken(
        string userId,
        string? tokenHash = null,
        DateTime? expiresAt = null,
        string? createdByIp = "127.0.0.1")
    {
        var now = DateTime.UtcNow;
        return RefreshToken.Create(
            userId,
            tokenHash ?? Guid.NewGuid().ToString("N"),
            expiresAt ?? now.AddDays(7),
            createdByIp,
            now);
    }
}