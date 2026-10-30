namespace Portfolio.DataAccess.Postgres;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations | dotnet ef database update)
/// (e.g. <c>dotnet ef migrations</c> / <c>dotnet ef database update</c>).
/// Delegates connection-string composition to <see cref="PostgresConnectionStringBuilder"/>
/// so design-time and runtime always behave identically.
/// </summary>
public class PortfolioDbContextFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        // Design-time has no DI container, so we build the configuration manually
        // (reading strictly from environment variables).
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = PostgresConnectionStringBuilder.Build(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<PortfolioDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PortfolioDbContext(optionsBuilder.Options);
    }
}