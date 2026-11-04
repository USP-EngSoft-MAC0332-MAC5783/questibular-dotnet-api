namespace Portfolio.Api.Composition.Extensions;

public static class SecurityAdaptersExtensions
{
    public static IServiceCollection AddSecurityAdapters(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddDataProtection();

        services.AddSingleton<Microsoft.AspNetCore.Identity.IPasswordHasher<object>, GenericPasswordHasher>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
        services.AddSingleton<ITotpProvider, OtpNetTotpProvider>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}