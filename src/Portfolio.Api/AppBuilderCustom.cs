namespace Portfolio.Api;

/// <summary>
/// Concrete builder for applications that want to use the default configuration with CORS, Kestrel, Swagger, and Warmup.
/// </summary>
public class AppBuilderCustom : AppBuilder<AppBuilderCustom>
{
    private AppBuilderCustom(string[] args) : base(args) { }

    public static AppBuilderCustom CreateBuilder(string[] args) => new(args);

    /// <summary>
    /// Custom application configurations (e.g., middleware pipeline, etc.).
    /// This method is called automatically in Build().
    /// </summary>
    public override AppBuilderCustom Configure()
    {
        var isDevelopment = AppBuilder<AppBuilderCustom>.IsDevelopment();
        var config = _builder.Configuration;

        UseHttpsRedirection(!isDevelopment); 

        // ── 1. ApiBase Configurations ────────
        ConfigureCors();
        ConfigureKestrel();

        // ── 2. Swagger ──────────── 
        ConfigureSwagger();
        _builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; 
            options.SubstituteApiVersionInUrl = true;
        });

        // ── 3. Database (Hexagonal Adapter) ────────────
        _builder.Services.AddPostgresDataAccess(config);
        RegisterMappings();

        // ── 4. Security (Hexagonal Adapter) ────────────
        _builder.Services.AddSecurityAdapters(config);
        _builder.Services.AddPortfolioAuthorization();

        // ── 5. Application Services (Hexagonal Ports) ────────────
        _builder.Services.AddDomainEvents();
        _builder.Services.AddAuthFeature(config);
        _builder.Services.AddUsersFeature();

        ConfigureJwtAuthentication();

        _builder.Services.AddHttpContextAccessor();
        _builder.Services.AddScoped<IApplicationContextService, ApplicationContextService>();

        UseWarmup();
        
        return this;
    }

    private void ConfigureJwtAuthentication()
    {
        _builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
            });

        _builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                var key = Encoding.UTF8.GetBytes(jwt.Secret);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        ConfigureAuthorization();
        UseAuthentication();
        UseAuthorization();
    }

    private void RegisterMappings()
    {
        using (var scope = _builder.Services.BuildServiceProvider().CreateScope())
        {
            MappingConfig.RegisterMappings();
        }
    }
}