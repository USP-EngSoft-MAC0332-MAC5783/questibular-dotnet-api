namespace Portfolio.DataAccess.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="PostgresRefreshTokenRepository"/>.
/// Focuses on behaviors that depend on real SQL semantics:
/// <c>ExecuteUpdateAsync</c> (bulk UPDATE), FK cascade, unique indexes.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class PostgresRefreshTokenRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public PostgresRefreshTokenRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Seeds a user (needed for FK constraint) and returns its ID.
    /// </summary>
    private async Task<string> SeedUserAsync()
    {
        await using var ctx = _fixture.CreateDbContext();
        var user = TestData.CreateUser();
        await new PostgresUserRepository(ctx).AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    [Fact]
    public async Task AddAsync_PersistsToken_AndRoundTripsAllFields()
    {
        var userId = await SeedUserAsync();
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresRefreshTokenRepository(ctx);
        var token = TestData.CreateRefreshToken(userId);

        await repo.AddAsync(token, CancellationToken.None);

        await using var verifyCtx = _fixture.CreateDbContext();
        var reloaded = await new PostgresRefreshTokenRepository(verifyCtx)
            .GetByHashAsync(token.TokenHash, CancellationToken.None);

        reloaded.Should().NotBeNull();
        reloaded!.Id.Should().Be(token.Id);
        reloaded.UserId.Should().Be(userId);
        reloaded.TokenHash.Should().Be(token.TokenHash);
        reloaded.ExpiresAt.Should().BeCloseTo(token.ExpiresAt, TimeSpan.FromMilliseconds(10));
        reloaded.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_Throws_WhenTokenHashIsDuplicated()
    {
        // Guardrail: UNIQUE index on TokenHash must exist.
        var userId = await SeedUserAsync();
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresRefreshTokenRepository(ctx);
        var hash = Guid.NewGuid().ToString("N");

        await repo.AddAsync(TestData.CreateRefreshToken(userId, tokenHash: hash), CancellationToken.None);
        var act = () => repo.AddAsync(TestData.CreateRefreshToken(userId, tokenHash: hash), CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task UpdateAsync_PersistsRevocationFields_Only()
    {
        var userId = await SeedUserAsync();
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresRefreshTokenRepository(ctx);
        var token = TestData.CreateRefreshToken(userId);
        await repo.AddAsync(token, CancellationToken.None);

        token.Revoke(DateTime.UtcNow, "10.0.0.1", replacedByTokenHash: "new-hash");
        await repo.UpdateAsync(token, CancellationToken.None);

        await using var verifyCtx = _fixture.CreateDbContext();
        var reloaded = await new PostgresRefreshTokenRepository(verifyCtx)
            .GetByHashAsync(token.TokenHash, CancellationToken.None);

        reloaded!.RevokedAt.Should().NotBeNull();
        reloaded.RevokedByIp.Should().Be("10.0.0.1");
        reloaded.ReplacedByTokenHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task RevokeAllForUserAsync_RevokesOnlyActiveTokens_OfThatUser()
    {
        // This is the key test for ExecuteUpdateAsync: verifies the WHERE clause
        // AND that the bulk UPDATE doesn't touch other users.
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync();

        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresRefreshTokenRepository(ctx);

        var activeToken = TestData.CreateRefreshToken(userId);
        var alreadyRevoked = TestData.CreateRefreshToken(userId);
        alreadyRevoked.Revoke(DateTime.UtcNow.AddDays(-1), "1.1.1.1");
        var otherUserToken = TestData.CreateRefreshToken(otherUserId);

        await repo.AddAsync(activeToken, CancellationToken.None);
        await repo.AddAsync(alreadyRevoked, CancellationToken.None);
        await repo.AddAsync(otherUserToken, CancellationToken.None);

        var now = DateTime.UtcNow;
        await repo.RevokeAllForUserAsync(userId, "10.0.0.1", now, CancellationToken.None);

        await using var verifyCtx = _fixture.CreateDbContext();
        var verifyRepo = new PostgresRefreshTokenRepository(verifyCtx);

        (await verifyRepo.GetByHashAsync(activeToken.TokenHash, CancellationToken.None))!
            .RevokedAt.Should().NotBeNull("active token must be revoked");
        (await verifyRepo.GetByHashAsync(activeToken.TokenHash, CancellationToken.None))!
            .RevokedByIp.Should().Be("10.0.0.1");

        (await verifyRepo.GetByHashAsync(alreadyRevoked.TokenHash, CancellationToken.None))!
            .RevokedAt.Should().BeCloseTo(alreadyRevoked.RevokedAt!.Value, TimeSpan.FromSeconds(1),
                "already-revoked tokens must not be touched again");

        (await verifyRepo.GetByHashAsync(otherUserToken.TokenHash, CancellationToken.None))!
            .RevokedAt.Should().BeNull("other users' tokens must remain untouched");
    }

    [Fact]
    public async Task DeletingUser_CascadesToRefreshTokens()
    {
        // Guardrail: FK CASCADE must be configured (and applied by the migration).
        var userId = await SeedUserAsync();
        await using var ctx = _fixture.CreateDbContext();
        await new PostgresRefreshTokenRepository(ctx)
            .AddAsync(TestData.CreateRefreshToken(userId), CancellationToken.None);

        // Delete the user directly via EF (no Delete repo method exists yet)
        await using (var deleteCtx = _fixture.CreateDbContext())
        {
            var user = await deleteCtx.Users.FindAsync([userId]);
            deleteCtx.Users.Remove(user!);
            await deleteCtx.SaveChangesAsync();
        }

        await using var verifyCtx = _fixture.CreateDbContext();
        verifyCtx.RefreshTokens.Should().BeEmpty("cascade must remove dependent tokens");
    }
}