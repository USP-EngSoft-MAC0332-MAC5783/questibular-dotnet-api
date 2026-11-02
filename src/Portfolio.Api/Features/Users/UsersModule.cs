namespace Portfolio.Api.Features.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}