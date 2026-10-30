namespace Portfolio.DataAccess.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="PostgresUserRepository"/>.
/// Each test exercises the real Postgres schema (created by migrations)
/// and the real Mapster mapping — the two things that unit tests cannot verify.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class PostgresUserRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public PostgresUserRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_PersistsUser_AndRoundTripsAllFields()
    {
        // Arrange
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresUserRepository(ctx);
        var user = TestData.CreateUser();
        user.SetupTwoFactor("protected-secret");           // exercise nullable field
        user.RegisterFailedAccess(maxAttempts: 1, lockoutDuration: TimeSpan.FromMinutes(5), DateTime.UtcNow);

        // Act
        await repo.AddAsync(user, CancellationToken.None);

        // Assert — reload through the repo to also cover GetByIdAsync
        await using var verifyCtx = _fixture.CreateDbContext();
        var reloaded = await new PostgresUserRepository(verifyCtx)
            .GetByIdAsync(user.Id, CancellationToken.None);

        reloaded.Should().NotBeNull();
        reloaded!.Email.Should().Be(user.Email);
        reloaded.PasswordHash.Should().Be(user.PasswordHash);
        reloaded.Roles.Should().BeEquivalentTo(user.Roles);
        reloaded.TwoFactorSecret.Should().Be("protected-secret");
        reloaded.AccessFailedCount.Should().Be(1);
        reloaded.LockoutEnd.Should().NotBeNull("failed access should trigger the lockout");
        reloaded.CreatedAt.Should().BeCloseTo(user.CreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        // Arrange — repo normalizes input to lowercase before querying.
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresUserRepository(ctx);
        var user = TestData.CreateUser(email: "joao@example.com");
        await repo.AddAsync(user, CancellationToken.None);

        // Act — query with mixed case
        var found = await repo.GetByEmailAsync("JOAO@Example.COM", CancellationToken.None);

        // Assert
        found.Should().NotBeNull("normalization must match storage convention");
        found!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
    {
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresUserRepository(ctx);

        var result = await repo.GetByEmailAsync("missing@example.com", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_Throws_WhenEmailIsDuplicated()
    {
        // Guardrail: the UNIQUE index on Email must exist in the migration.
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresUserRepository(ctx);

        await repo.AddAsync(TestData.CreateUser(email: "dup@example.com"), CancellationToken.None);

        var act = () => repo.AddAsync(TestData.CreateUser(email: "dup@example.com"), CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();   // Npgsql unique violation wrapped by EF
    }

    [Fact]
    public async Task UpdateAsync_PersistsEveryMutableField()
    {
        // Guards against forgetting a property in UpdateAsync — the classic silent bug.
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresUserRepository(ctx);
        var user = TestData.CreateUser();
        await repo.AddAsync(user, CancellationToken.None);

        // Mutate via domain methods only
        user.RegisterSuccessfulLogin(DateTime.UtcNow);
        user.SetupTwoFactor("new-secret");
        user.EnableTwoFactor(["code1", "code2"], DateTime.UtcNow);

        await repo.UpdateAsync(user, CancellationToken.None);

        // Reload and verify state survived the round-trip
        await using var verifyCtx = _fixture.CreateDbContext();
        var reloaded = await new PostgresUserRepository(verifyCtx)
            .GetByIdAsync(user.Id, CancellationToken.None);

        reloaded!.TwoFactorEnabled.Should().BeTrue();
        reloaded.TwoFactorSecret.Should().Be("new-secret");
        reloaded.TwoFactorRecoveryCodes.Should().BeEquivalentTo(["code1", "code2"]);
        reloaded.TwoFactorEnabledAt.Should().NotBeNull();
        reloaded.LastLoginAt.Should().NotBeNull();
        reloaded.AccessFailedCount.Should().Be(0);
        reloaded.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DoesNotThrow_WhenUserDoesNotExist()
    {
        // Contract: Update is a no-op for missing entities (current implementation).
        await using var ctx = _fixture.CreateDbContext();
        var repo = new PostgresUserRepository(ctx);
        var user = TestData.CreateUser();

        var act = () => repo.UpdateAsync(user, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}