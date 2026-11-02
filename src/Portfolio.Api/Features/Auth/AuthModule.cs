namespace Portfolio.Api.Features.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuthFacade, AuthFacade>();
        return services;
    }
}