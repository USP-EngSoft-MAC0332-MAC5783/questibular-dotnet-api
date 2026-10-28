namespace Portfolio.ApiBase.Configuration;

/// <summary>
/// Configures the application's CORS policy based on appsettings.json and environment variables.
/// </summary>
/// <remarks>
/// Environment variables take precedence over the JSON configuration to support containerized deployments.
/// </remarks>
public static partial class CorsPolicyConfigurator
{
    public const string ENV_CORS_ALLOWED_ORIGINS = "CORS_ALLOWED_ORIGINS";
    public const string ENV_CORS_ALLOWED_METHODS = "CORS_ALLOWED_METHODS";
    public const string ENV_ALLOW_HEADERS = "ALLOW_HEADERS";
    public const string ENV_ALLOW_EXPOSED_HEADERS = "ALLOW_EXPOSED_HEADERS";
    public const string ENV_ALLOW_CREDENTIALS = "ALLOW_CREDENTIALS";
    public const string ENV_PREFLIGHT_MAX_AGE_IN_SECONDS = "PREFLIGHT_MAX_AGE_IN_SECONDS";

    /// <summary>
    /// Builds the CORS policy, merging JSON settings and environment variables.
    /// </summary>
    public static CorsPolicy Configure(
        IConfiguration configuration, 
        ILogger logger, 
        Action<CorsPolicyBuilder>? configure = null)
    {
        var corsSettings = LoadCorsSettings(configuration, logger);
        return BuildCorsPolicy(corsSettings, configure, logger);
    }

    /// <summary>
    /// Loads CORS configurations from appsettings.json and overrides them with env vars.
    /// </summary>
    public static CorsSettings LoadCorsSettings(IConfiguration configuration, ILogger logger)
    {
        var settings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();

        // override with env vars
        string? originsRaw = configuration[ENV_CORS_ALLOWED_ORIGINS];
        if (!string.IsNullOrEmpty(originsRaw))
        {
            settings = settings with
            {
                AllowedOrigins = originsRaw.Split([';', ','], StringSplitOptions.RemoveEmptyEntries)
            };
            LogCorsOriginsOverridden(logger, originsRaw);
        }

        string? methodsRaw = configuration[ENV_CORS_ALLOWED_METHODS];
        if (!string.IsNullOrEmpty(methodsRaw))
        {
            settings = settings with
            {
                AllowedMethods = methodsRaw.Split([';', ','], StringSplitOptions.RemoveEmptyEntries)
            };
            LogCorsMethodsOverridden(logger, methodsRaw);
        }

        string? headersRaw = configuration[ENV_ALLOW_HEADERS];
        if (!string.IsNullOrEmpty(headersRaw))
        {
            settings = settings with
            {
                AllowedHeaders = headersRaw.Split([';', ','], StringSplitOptions.RemoveEmptyEntries)
            };
            LogCorsHeadersOverridden(logger, headersRaw);
        }

        string? exposedRaw = configuration[ENV_ALLOW_EXPOSED_HEADERS];
        if (!string.IsNullOrEmpty(exposedRaw))
        {
            settings = settings with
            {
                AllowedExposedHeaders = exposedRaw.Split([';', ','], StringSplitOptions.RemoveEmptyEntries)
            };
            LogCorsExposedHeadersOverridden(logger, exposedRaw);
        }

        string? credentialsRaw = configuration[ENV_ALLOW_CREDENTIALS];
        if (!string.IsNullOrEmpty(credentialsRaw) && bool.TryParse(credentialsRaw, out bool creds))
        {
            settings = settings with { AllowCredentials = creds };
            LogCorsCredentialsOverridden(logger, creds);
        }

        string? maxAgeRaw = configuration[ENV_PREFLIGHT_MAX_AGE_IN_SECONDS];
        if (!string.IsNullOrEmpty(maxAgeRaw) && int.TryParse(maxAgeRaw, out int maxAge) && maxAge > 0)
        {
            settings = settings with { PreflightMaxAgeInSeconds = maxAge };
            LogCorsPreflightMaxAgeOverridden(logger, maxAge);
        }

        return settings;
    }

    /// <summary>
    /// Builds the actual CORS policy based on the loaded settings.
    /// </summary>
    private static CorsPolicy BuildCorsPolicy(
        CorsSettings settings,
        Action<CorsPolicyBuilder>? configure,
        ILogger logger)
    {
        var builder = new CorsPolicyBuilder();

        // 1. ORIGINS
        if (settings.AllowedOrigins.Length > 0)
        {
            builder.WithOrigins(settings.AllowedOrigins);
        }
        else
        {
            LogNoOriginsConfigured(logger);
        }

        // 2. HTTP METHODS
        if (settings.AllowedMethods.Length > 0)
        {
            builder.WithMethods(settings.AllowedMethods);
        }
        else
        {
            LogNoMethodsConfigured(logger);
            builder.AllowAnyMethod();
        }

        // 3. ALLOWED HEADERS
        if (settings.AllowedHeaders.Length > 0)
        {
            builder.WithHeaders(settings.AllowedHeaders);
        }
        else
        {
            LogNoHeadersConfigured(logger);
            builder.AllowAnyHeader();
        }

        // 4. EXPOSED HEADERS (optional)
        if (settings.AllowedExposedHeaders.Length > 0)
        {
            builder.WithExposedHeaders(settings.AllowedExposedHeaders);
        }

        // 5. CREDENTIALS (cookies, tokens)
        if (settings.AllowCredentials)
        {
            // Cannot be used with AllowAnyOrigin; .NET throws an exception.
            if (settings.AllowedOrigins.Length > 0)
            {
                builder.AllowCredentials();
                LogCorsCredentialsEnabled(logger);
            }
            else
            {
                LogCorsCredentialsIgnored(logger);
            }
        }

        // 6. PREFLIGHT CACHE TIME (OPTIONS)
        if (settings.PreflightMaxAgeInSeconds > 0)
        {
            builder.SetPreflightMaxAge(TimeSpan.FromSeconds(settings.PreflightMaxAgeInSeconds));
            LogCorsPreflightMaxAgeConfigured(logger, settings.PreflightMaxAgeInSeconds);
        }

        // 7. EXTRA CUSTOMIZATION PROVIDED BY THE CALLER
        configure?.Invoke(builder);

        return builder.Build();
    }

    #region Logs (source-generated)

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS Origins overridden by env var: {Origins}")]
    private static partial void LogCorsOriginsOverridden(ILogger logger, string origins);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS Methods overridden by env var: {Methods}")]
    private static partial void LogCorsMethodsOverridden(ILogger logger, string methods);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS Headers overridden by env var: {Headers}")]
    private static partial void LogCorsHeadersOverridden(ILogger logger, string headers);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS ExposedHeaders overridden by env var: {Headers}")]
    private static partial void LogCorsExposedHeadersOverridden(ILogger logger, string headers);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS AllowCredentials overridden by env var: {Creds}")]
    private static partial void LogCorsCredentialsOverridden(ILogger logger, bool creds);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS PreflightMaxAge overridden by env var: {MaxAge}s")]
    private static partial void LogCorsPreflightMaxAgeOverridden(ILogger logger, int maxAge);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No CORS origins configured. Allowing any origin (AllowAnyOrigin).")]
    private static partial void LogNoOriginsConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No CORS methods configured. Allowing any method (AllowAnyMethod).")]
    private static partial void LogNoMethodsConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No CORS headers configured. Allowing any header (AllowAnyHeader).")]
    private static partial void LogNoHeadersConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS AllowCredentials enabled.")]
    private static partial void LogCorsCredentialsEnabled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AllowCredentials=true ignored because AllowAnyOrigin is active. Define specific origins to use credentials.")]
    private static partial void LogCorsCredentialsIgnored(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "CORS PreflightMaxAge configured to {MaxAge}s")]
    private static partial void LogCorsPreflightMaxAgeConfigured(ILogger logger, int maxAge);
    
    #endregion
}