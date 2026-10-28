namespace Portfolio.ApiBase.Settings;

/// <summary>
/// CORS configuration structure, can be populated by appsettings.json and env vars.
/// </summary>
public record class CorsSettings
{
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
    public string[] AllowedMethods { get; init; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; init; } = Array.Empty<string>();
    public string[] AllowedExposedHeaders { get; init; } = Array.Empty<string>();
    public bool AllowCredentials { get; init; } = false;
    public int PreflightMaxAgeInSeconds { get; init; } = 0;

    public CorsSettings() { }
}