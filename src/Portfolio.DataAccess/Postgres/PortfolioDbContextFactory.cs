namespace Portfolio.DataAccess.Postgres;

/// <summary>
/// Design-time factory used by EF Core tooling (e.g. <c>dotnet ef migrations</c> / <c>dotnet ef database update</c>).
/// It builds the <see cref="PortfolioDbContext"/> by composing the PostgreSQL connection
/// string from individual environment variables.
/// </summary>
public class PortfolioDbContextFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    private const string UserEnvVar     = "POSTGRES_USER";
    private const string PasswordEnvVar = "POSTGRES_PASSWORD";
    private const string HostEnvVar     = "POSTGRES_HOST";
    private const string PortEnvVar     = "POSTGRES_PORT";     // optional (defaults to 5432)
    private const string DatabaseEnvVar = "POSTGRES_DATABASE";
    private const string EnvNameVar     = "ASPNETCORE_ENVIRONMENT";

    public PortfolioDbContext CreateDbContext(string[] args)
    {
        // Just build the configuration reading strictly from ENV VARS
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var user     = configuration[UserEnvVar];
        var password = configuration[PasswordEnvVar];
        var host     = configuration[HostEnvVar];
        var database = configuration[DatabaseEnvVar];
        var port     = configuration[PortEnvVar];

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(user))     missing.Add(UserEnvVar);
        if (string.IsNullOrWhiteSpace(password)) missing.Add(PasswordEnvVar);
        if (string.IsNullOrWhiteSpace(host))     missing.Add(HostEnvVar);
        if (string.IsNullOrWhiteSpace(database)) missing.Add(DatabaseEnvVar);

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required environment variable(s) to build the PostgreSQL connection string: " +
                $"{string.Join(", ", missing)}. " +
                $"Please set {UserEnvVar}, {PasswordEnvVar}, {HostEnvVar}, {DatabaseEnvVar} " +
                $"(and optionally {PortEnvVar}).");
        }

        var environment = configuration[EnvNameVar] ?? string.Empty;
        var isDevelopment = environment.Contains("dev", StringComparison.OrdinalIgnoreCase);

        // Build the connection string using NpgsqlConnectionStringBuilder
        // (avoids manual escaping issues with passwords/special chars).
        var connectionBuilder = new NpgsqlConnectionStringBuilder
        {
            Host               = host,
            Port               = string.IsNullOrWhiteSpace(port) ? 5432 : int.Parse(port),
            Username           = user,
            Password           = password,
            Database           = database,
            Pooling            = true,
            IncludeErrorDetail = isDevelopment,
        };

        var connectionString = connectionBuilder.ConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<PortfolioDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PortfolioDbContext(optionsBuilder.Options);
    }
}