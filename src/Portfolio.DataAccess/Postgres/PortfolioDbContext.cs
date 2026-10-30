namespace Portfolio.DataAccess.Postgres;

/// <summary>
/// Entity Framework Core database context.
/// It represents the session with the PostgreSQL database and exposes the
/// shared entity sets used across the data access layer.
/// Entity mappings are discovered automatically.
/// </summary>
public class PortfolioDbContext : DbContext
{
    // Shared Models References
    public DbSet<UserDbModel> Users => Set<UserDbModel>();
    public DbSet<RefreshTokenDbModel> RefreshTokens => Set<RefreshTokenDbModel>();

    /// <summary>
    /// Initializes a new instance of the <see cref="PortfolioDbContext"/> class
    /// using the provided options (connection string, provider, behaviors, etc.).
    /// </summary>
    /// <param name="options">The options used to configure the DbContext.</param>
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options) { }

    /// <summary>
    /// Configures the model by applying all entity type configurations
    /// (<see cref="IEntityTypeConfiguration{TEntity}"/>) found in the
    /// "Configurations" folder of this project's assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans the current assembly and applies all entity configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }
}