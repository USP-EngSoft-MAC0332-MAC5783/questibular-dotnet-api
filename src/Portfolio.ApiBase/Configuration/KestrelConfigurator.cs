namespace Portfolio.ApiBase.Configuration;

/// <summary>
/// Configures Kestrel HTTP/HTTPS endpoints in a decoupled way from the AppBuilder.
/// </summary>
public static partial class KestrelConfigurator
{
    public const string ENV_HTTP_PORT = "KESTREL_HTTP_PORT";
    public const string ENV_HTTPS_PORT = "KESTREL_HTTPS_PORT";

    /// <summary>
    /// Configures Kestrel with HTTP and HTTPS endpoints.
    /// The caller is responsible for setting up the HTTPS delegate (e.g., using <see cref="HttpsConfigurator"/>).
    /// </summary>
    public static void Configure(
        KestrelServerOptions options,
        IConfiguration configuration,
        ILogger logger,
        Action<ListenOptions>? configureHttp = null,
        Action<ListenOptions>? configureHttps = null,
        Action<KestrelServerOptions>? additionalConfig = null)
    {
        // Read ports from appsettings.json with default fallbacks
        var httpPort = configuration.GetValue<int>("Kestrel:HttpPort", 5000);
        var httpsPort = configuration.GetValue<int>("Kestrel:HttpsPort", 5001);

        // Override with environment variables
        var envHttp = configuration[ENV_HTTP_PORT];
        if (!string.IsNullOrEmpty(envHttp) && int.TryParse(envHttp, out var envPort))
        {
            httpPort = envPort;
            LogHttpPortOverridden(logger, httpPort);
        }

        var envHttps = configuration[ENV_HTTPS_PORT];
        if (!string.IsNullOrEmpty(envHttps) && int.TryParse(envHttps, out var envHttpsPort))
        {
            httpsPort = envHttpsPort;
            LogHttpsPortOverridden(logger, httpsPort);
        }

        // HTTP Endpoint
        if (httpPort > 0)
        {
            options.ListenAnyIP(httpPort, listenOptions =>
            {
                LogConfiguringHttp(logger, httpPort);
                configureHttp?.Invoke(listenOptions);
            });
        }

        // HTTPS Endpoint (caller defines how to configure TLS)
        if (httpsPort > 0)
        {
            options.ListenAnyIP(httpsPort, listenOptions =>
            {
                LogConfiguringHttps(logger, httpsPort);
                configureHttps?.Invoke(listenOptions);
            });
        }

        // Additional configurations (limits, timeouts, etc.)
        additionalConfig?.Invoke(options);
    }

    /// <summary>
    /// Extension method to be used directly on the builder if needed.
    /// </summary>
    public static void ConfigureKestrel(
        this KestrelServerOptions options,
        IConfiguration configuration,
        ILogger logger,
        Action<ListenOptions>? configureHttp = null,
        Action<ListenOptions>? configureHttps = null,
        Action<KestrelServerOptions>? additionalConfig = null)
    {
        Configure(options, configuration, logger, configureHttp, configureHttps, additionalConfig);
    }

    #region Logs (source-generated)

    [LoggerMessage(Level = LogLevel.Information, Message = "Kestrel HTTP Port overridden by env var: {Port}")]
    private static partial void LogHttpPortOverridden(ILogger logger, int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "Kestrel HTTPS Port overridden by env var: {Port}")]
    private static partial void LogHttpsPortOverridden(ILogger logger, int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "Configuring HTTP on port {Port}")]
    private static partial void LogConfiguringHttp(ILogger logger, int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "Configuring HTTPS on port {Port}")]
    private static partial void LogConfiguringHttps(ILogger logger, int port);

    #endregion
}