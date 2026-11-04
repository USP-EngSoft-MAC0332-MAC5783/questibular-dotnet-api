namespace Portfolio.Api.Composition.Extensions;

public static class DomainEventsExtensions
{
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IDomainEventHandler<UserRegisteredEvent>, SendWelcomeEmailHandler>();

        return services;
    }
}