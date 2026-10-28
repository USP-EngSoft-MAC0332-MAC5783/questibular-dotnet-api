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
        return this;
    }
}