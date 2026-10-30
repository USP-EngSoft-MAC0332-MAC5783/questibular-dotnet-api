namespace Portfolio.DataAccess.Tests.Fixtures;

/// <summary>
/// Boots a throwaway PostgreSQL container for the whole test run and
/// applies all EF Core migrations once. Each test class gets a clean slate
/// through <see cref="ResetAsync"/>, but the container itself is reused —
/// spinning it up is the slow part (~5s), so we pay that cost only once.
/// <para>
/// This fixture deliberately bypasses <see cref="PortfolioDbContextFactory"/>
/// (which is design-time only and reads <c>POSTGRES_*</c> env vars); tests build
/// the <see cref="PortfolioDbContext"/> directly against the container's
/// connection string.
/// </para>
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    static PostgresFixture()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DataAccessExtensions).Assembly);
    }

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("portfolio_tests")
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    /// <summary>
    /// Creates a fresh <see cref="PortfolioDbContext"/> connected to the test container.
    /// Caller owns disposal — always wrap in <c>await using</c>.
    /// </summary>
    internal PortfolioDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .EnableSensitiveDataLogging()
            .Options;

        return new PortfolioDbContext(options);
    }

    /// <summary>
    /// Starts the container and applies all migrations.
    /// Running migrations here (instead of on each test) also serves as a
    /// smoke test: if a migration is broken, the whole suite fails fast
    /// with a clear error.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var ctx = CreateDbContext();
        await ctx.Database.MigrateAsync();
    }

    /// <summary>
    /// Wipes all business data while preserving schema and
    /// <c>__EFMigrationsHistory</c>. Called between tests so each one
    /// starts from a known state.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var ctx = CreateDbContext();
        await ctx.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"Users\", \"RefreshTokens\" RESTART IDENTITY CASCADE;");
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

/// <summary>
/// xUnit collection definition — sharing a single <see cref="PostgresFixture"/>
/// across all test classes in the collection, and running them **sequentially**
/// (the default for collections) to avoid TRUNCATE races.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}