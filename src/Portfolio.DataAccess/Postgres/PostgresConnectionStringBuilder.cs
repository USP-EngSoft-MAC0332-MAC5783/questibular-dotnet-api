namespace Portfolio.DataAccess.Postgres;

/// <summary>
/// Builds the PostgreSQL connection string from individual environment variables.
/// Shared between the design-time factory (EF CLI) and the runtime DI registration
/// to guarantee both use exactly the same rules.
/// </summary>
internal static class PostgresConnectionStringBuilder
{
    private const string UserEnvVar     = "POSTGRES_USER";
    private const string PasswordEnvVar = "POSTGRES_PASSWORD";
    private const string HostEnvVar     = "POSTGRES_HOST";
    private const string PortEnvVar     = "POSTGRES_PORT";     // optional (defaults to 5432)
    private const string DatabaseEnvVar = "POSTGRES_DATABASE";
    private const string EnvNameVar     = "ASPNETCORE_ENVIRONMENT";

    /// <summary>
    /// Composes the connection string from the <c>POSTGRES_*</c> environment variables.
    /// Throws <see cref="InvalidOperationException"/> if any required variable is missing
    /// (fail-fast: the app should not start with an incomplete configuration).
    /// </summary>
    public static string Build(IConfiguration configuration)
    {
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

        // IncludeErrorDetail only in development to avoid leaking sensitive data in prod.
        var environment   = configuration[EnvNameVar] ?? string.Empty;
        var isDevelopment = environment.Contains("dev", StringComparison.OrdinalIgnoreCase);

        return new NpgsqlConnectionStringBuilder
        {
            Host               = host,
            Port               = string.IsNullOrWhiteSpace(port) ? 5432 : int.Parse(port),
            Username           = user,
            Password           = password,
            Database           = database,
            Pooling            = true,
            IncludeErrorDetail = isDevelopment,
        }.ConnectionString;
    }
}