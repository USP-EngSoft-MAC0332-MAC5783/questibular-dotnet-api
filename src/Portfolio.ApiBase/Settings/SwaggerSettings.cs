namespace Portfolio.ApiBase.Settings;

public record class SwaggerSettings
{
    public string Title { get; init; } = ".NET API";
    public string Description { get; init; } = "API description";
    public string Version { get; init; } = "v1";
    public string? ContactName { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactUrl { get; init; }
    public bool EnableJwtAuth { get; init; } = false;
    public string? AuthSchemeName { get; init; } = "Bearer";

    public SwaggerSettings() { }
}