namespace Portfolio.Api.Composition.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPortfolioAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", p => p.RequireRole("Admin"));
            options.AddPolicy("SelfOnly", p => p.Requirements.Add(new SelfOnlyRequirement()));
            options.AddPolicy("Teacher", p => p.Requirements.Add(new SelfOnlyRequirement()));
            options.AddPolicy("Student", p => p.Requirements.Add(new SelfOnlyRequirement()));
        });

        services.AddSingleton<IAuthorizationHandler, SelfOnlyHandler>();
        return services;
    }
}