namespace Portfolio.ApiBase.Configuration;

/// <summary>
/// Configures the application's logging providers.
/// </summary>
public static class LoggingConfigurator
{
    /// <summary>
    /// Sets up default logging providers and minimum levels.
    /// </summary>
    /// <param name="logging">The logging builder.</param>
    /// <param name="configure">Additional customizations.</param>
    /// <param name="clearProviders">
    /// If true, removes all previously registered providers (host defaults). 
    /// Default is false to avoid breaking consumer's telemetry (e.g., Application Insights, Serilog).
    /// </param>
    public static void Configure(ILoggingBuilder logging, Action<ILoggingBuilder>? configure = null, bool clearProviders = false)
    {
        // Only clear if explicitly requested. Libraries should generally not wipe host logging.
        if (clearProviders)
        {
            logging.ClearProviders();
        }

        logging.AddConsole();
        logging.AddDebug();

        // Adds minimum log level filter
        logging.SetMinimumLevel(LogLevel.Information);

        // Allows overriding with custom configurations
        configure?.Invoke(logging);
    }
}