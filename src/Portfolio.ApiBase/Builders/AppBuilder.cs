namespace Portfolio.ApiBase.Builders;

/// <summary>
/// Base builder for configuring and constructing the WebApplication.
/// Uses the Fluent Builder pattern to abstract the ASP.NET Core pipeline setup.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type.</typeparam>
public abstract partial class AppBuilder<TBuilder> where TBuilder: AppBuilder<TBuilder>
{
    protected readonly WebApplicationBuilder _builder;
    protected readonly ILogger _logger;
    private bool _useHttpsRedirection = true;
    private bool _useCors = false;
    private bool _useSwagger = true;
    private bool _useAuthentication = false;
    private bool _useAuthorization = false;
    protected Action<SwaggerUIOptions>? _swaggerUiCustomization;

    protected AppBuilder(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);
        // Temp logger for configuration phase, before the DI container is built.
        var loggerFactory = LoggerFactory.Create(opt => opt.AddConsole());
        _logger = loggerFactory.CreateLogger(GetType().Name);
    }

    /// <summary>
    /// Configures CORS policies.
    /// </summary>
    protected TBuilder ConfigureCors(Action<CorsPolicyBuilder>? customPolicy = null)
    {
        var corsPolicy = CorsPolicyConfigurator.Configure(_builder.Configuration, _logger, customPolicy);
        _builder.Services.AddSingleton(corsPolicy);
        _builder.Services.AddCors(options => options.AddDefaultPolicy(corsPolicy));
        _useCors = true;
        return (TBuilder)this;
    }

    /// <summary>
    /// Configures Kestrel endpoints (HTTP/HTTPS).
    /// </summary>
    protected TBuilder ConfigureKestrel(
        Action<ListenOptions>? configureHttp = null,
        Action<ListenOptions>? configureHttps = null,
        Action<KestrelServerOptions>? additionalConfig = null,
        Action<HttpsConnectionAdapterOptions>? configureHttpsOptions = null,
        int? httpsPort = null,
        bool throwIfNoCertificate = true)
    {
        Action<ListenOptions>? effectiveConfigureHttps = configureHttps;

        // If the caller provided HTTPS options, a specific port, or didn't ask to throw, we wrap it to use our HttpsConfigurator.
        if (configureHttpsOptions != null || httpsPort.HasValue || !throwIfNoCertificate || configureHttps == null)
        {
            effectiveConfigureHttps = listenOptions =>
            {
                listenOptions.UseHttps(httpsOptions =>
                {
                    HttpsConfigurator.ConfigureHttpsOptions(
                        httpsOptions,
                        _builder.Configuration,
                        _logger,
                        configureHttpsOptions,
                        throwIfNoCertificate
                    );
                });

                // Invoke original delegate if provided
                configureHttps?.Invoke(listenOptions);
            };
        }

        _builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            KestrelConfigurator.Configure(
                serverOptions,
                _builder.Configuration,
                _logger,
                configureHttp,
                effectiveConfigureHttps,
                additionalConfig
            );
        });

        return (TBuilder)this;
    }

    /// <summary>
    /// Configures Swagger services and UI.
    /// </summary>
    protected TBuilder ConfigureSwagger(
        Action<SwaggerGenOptions>? configureSwaggerGen = null, 
        Action<SwaggerUIOptions>? configureSwaggerUI = null,
        bool enable = true)
    {
        _useSwagger = enable;
        if(_useSwagger)
        {
            _swaggerUiCustomization = configureSwaggerUI;

            SwaggerConfigurator.AddSwaggerServices(
                _builder.Services,
                _builder.Configuration,
                _logger,
                configureSwaggerGen
            );

            LogSwaggerConfigured(_logger);
        }

        return (TBuilder) this;
    }

    /// <summary>
    /// Configures Authorization services.
    /// </summary>
    protected TBuilder ConfigureAuthorization()
    {
        _builder.Services.AddAuthorization();
        _useAuthorization = true;
        return (TBuilder)this;
    }

    public abstract TBuilder Configure();
    
    /// <summary>
    /// Registers the warmup hosted service to execute initialization steps on startup.
    /// </summary>
    public virtual TBuilder UseWarmup()
    {
        _builder.Services.AddHostedService<WarmupService>();
        LogWarmupRegistered(_logger);

        return (TBuilder)this;
    }
    
    /// <summary>
    /// Enables or disables HTTPS redirection middleware.
    /// </summary>
    public virtual  TBuilder UseHttpsRedirection(bool enable = true)
    {
        _useHttpsRedirection = enable;
        return (TBuilder)this;
    }

    /// <summary>
    /// Enables the authentication middleware in the pipeline. 
    /// Requires authentication services (e.g., AddAuthentication/JwtBearer) to be registered.
    /// </summary>
    public virtual TBuilder UseAuthentication(bool enable = true)
    {
        _useAuthentication = enable;
        return (TBuilder)this;
    }

    /// <summary>
    /// Enables the authorization middleware in the pipeline. 
    /// Depends on UseAuthentication being called.
    /// </summary>
    public virtual TBuilder UseAuthorization(bool enable = true)
    {
        _useAuthorization = enable;
        return (TBuilder)this;
    }

    /// <summary>
    /// Builds and configures the WebApplication pipeline.
    /// </summary>
    public virtual WebApplication Build()
    {
        LogBuildingApplication(_logger);

        Configure();

        // Default services
        _builder.Services.AddControllers();
        _builder.Services.AddHealthChecks();

        var app = _builder.Build();

        // Middlewares
        if (_useHttpsRedirection)
            app.UseHttpsRedirection();

        if(_useCors)
            app.UseCors();

        if (_useAuthentication)
            app.UseAuthentication();

        if (_useAuthorization)
            app.UseAuthorization();

        if (_useSwagger)
            SwaggerConfigurator.UseSwaggerWithUI(app, _logger, _swaggerUiCustomization);

        app.MapHealthChecks("/health");
        app.MapControllers();

        LogApplicationConfiguredSuccessfully(_logger);

        return app;
    }

    /// <summary>
    /// Builds and runs the WebApplication.
    /// </summary>
    public virtual WebApplication Run()
    {
        var app = Build();
        app.Run();
        return app;
    }

    #region Logs (source-generated)

    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger configured")]
    private static partial void LogSwaggerConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Warmup registered.")]
    private static partial void LogWarmupRegistered(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Building application...")]
    private static partial void LogBuildingApplication(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Application configured successfully!")]
    private static partial void LogApplicationConfiguredSuccessfully(ILogger logger);

    #endregion
}