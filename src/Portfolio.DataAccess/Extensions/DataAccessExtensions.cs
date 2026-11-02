namespace Portfolio.DataAccess.Extensions;

/// <summary>
/// Registers the PostgreSQL data-access services (DbContext + repositories)
/// into the application's dependency injection container.
/// </summary>
public static class DataAccessExtensions
{
    /// <summary>
    /// Adds the <see cref="PortfolioDbContext"/> and the PostgreSQL repository implementations.
    /// The connection string is composed from <c>POSTGRES_*</c> environment variables
    /// (see <see cref="PostgresConnectionStringBuilder"/>).
    /// </summary>
    public static IServiceCollection AddPostgresDataAccess(
        this IServiceCollection services, IConfiguration configuration)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DataAccessExtensions).Assembly);
        
        // Build the connection string eagerly so a misconfigured environment
        // fails fast at startup — not on the first request that touches the DB.
        var connectionString = PostgresConnectionStringBuilder.Build(configuration);

        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Postgres takes ownership of the Domain repository contracts.
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();

        return services;
    }
}