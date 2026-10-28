namespace Portfolio.ApiBase.Configuration;

/// <summary>
/// Extension methods for configuring Kestrel ListenOptions.
/// Useful when manually setting up endpoints without the <see cref="Builders.AppBuilder{TBuilder}"/>.
/// </summary>
public static class KestrelExtensions
{
    /// <summary>
    /// Applies HTTPS configuration (via <see cref="HttpsConfigurator"/>) to a specific <see cref="ListenOptions"/> endpoint.
    /// </summary>
    public static void UseHttpsFromConfiguration(
        this ListenOptions listenOptions,
        IConfiguration configuration,
        ILogger logger)
    {
        listenOptions.UseHttps(httpsOptions =>
        {
            HttpsConfigurator.ConfigureHttpsOptions(httpsOptions, configuration, logger);
        });
    }
}