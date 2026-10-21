namespace Portfolio.ApiBase.Configuration;

/// <summary>
/// Configures Swagger generation and UI, reading settings from appsettings.json and environment variables.
/// </summary>
public static partial class SwaggerConfigurator
{
    public const string ENV_TITLE = "SWAGGER_TITLE";
    public const string ENV_DESCRIPTION = "SWAGGER_DESCRIPTION";
    public const string ENV_VERSION = "SWAGGER_VERSION";
    public const string ENV_ENABLE_AUTH = "SWAGGER_ENABLE_AUTH";

    /// <summary>
    /// Loads Swagger settings and applies environment variable overrides.
    /// </summary>
    private static SwaggerSettings LoadSettings(IConfiguration configuration, ILogger logger)
    {
        // Fixed: Using record class and Get<T> to prevent binder issues with structs.
        var settings = configuration.GetSection("Swagger").Get<SwaggerSettings>() ?? new SwaggerSettings();

        string? val;
        val = configuration[ENV_TITLE];
        if (!string.IsNullOrEmpty(val))
        {
            settings = settings with { Title = val };
            LogTitleOverridden(logger, val);
        }
        
        val = configuration[ENV_DESCRIPTION];
        if (!string.IsNullOrEmpty(val))
        {
            settings = settings with { Description = val };
            LogDescriptionOverridden(logger, val);
        }
        
        val = configuration[ENV_VERSION];
        if (!string.IsNullOrEmpty(val))
        {
            settings = settings with { Version = val };
            LogVersionOverridden(logger, val);
        }
        
        val = configuration[ENV_ENABLE_AUTH];
        if (!string.IsNullOrEmpty(val) && bool.TryParse(val, out var enableAuth))
        {
            settings = settings with { EnableJwtAuth = enableAuth };
            LogEnableAuthOverridden(logger, enableAuth);
        }
        
        return settings;
    }

    /// <summary>
    /// Registers Swagger generator services in the DI container.
    /// </summary>
    public static void AddSwaggerServices(
        IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        Action<SwaggerGenOptions>? configureGen = null)
    {
        var settings = LoadSettings(configuration, logger);

        OpenApiContact? contact = null;
        if (!string.IsNullOrEmpty(settings.ContactName))
        {
            Uri? contactUrl = string.IsNullOrEmpty(settings.ContactUrl) 
                ? null 
                : new Uri(settings.ContactUrl);

            contact = new OpenApiContact
            {
                Name = settings.ContactName,
                Email = settings.ContactEmail,
                Url = contactUrl
            };
        }

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(settings.Version, new OpenApiInfo
            {
                Title = settings.Title,
                Description = settings.Description,
                Version = settings.Version,
                Contact = contact
            });

            if (settings.EnableJwtAuth)
            {
                var schemeName = settings.AuthSchemeName ?? "Bearer";
                options.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
                {
                    Description = "Insert a JWT token: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = schemeName.ToLowerInvariant(),
                    BearerFormat = "JWT"
                });
            }

            configureGen?.Invoke(options);
        });
    }

    /// <summary>
    /// Adds the Swagger middleware and UI to the application pipeline.
    /// </summary>
    public static void UseSwaggerWithUI(
        WebApplication app,
        ILogger logger,
        Action<SwaggerUIOptions>? configureUI = null)
    {
        var settings = LoadSettings(app.Configuration, logger);

        app.UseSwagger();
        LogSwaggerMiddlewareRegistered(logger);

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{settings.Version}/swagger.json", $"{settings.Title} {settings.Version}");
            options.RoutePrefix = "swagger";
            configureUI?.Invoke(options);
        });

        LogSwaggerUiRegistered(logger, settings.Version);
    }

    // Extension methods for convenience
    public static void AddSwagger(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger,
        Action<SwaggerGenOptions>? configureGen = null)
    {
        AddSwaggerServices(services, configuration, logger, configureGen);
    }

    public static void UseSwagger(this WebApplication app, ILogger logger, Action<SwaggerUIOptions>? configureUI = null)
    {
        UseSwaggerWithUI(app, logger, configureUI);
    }

    #region Logs (source-generated)

    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger Title overridden by env var: {Title}")]
    private static partial void LogTitleOverridden(ILogger logger, string title);

    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger Description overridden by env var: {Description}")]
    private static partial void LogDescriptionOverridden(ILogger logger, string description);

    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger Version overridden by env var: {Version}")]
    private static partial void LogVersionOverridden(ILogger logger, string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger EnableAuth overridden by env var: {Enable}")]
    private static partial void LogEnableAuthOverridden(ILogger logger, bool enable);

    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger middleware registered.")]
    private static partial void LogSwaggerMiddlewareRegistered(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "SwaggerUI registered with endpoint for version {Version}.")]
    private static partial void LogSwaggerUiRegistered(ILogger logger, string version);

    #endregion
}