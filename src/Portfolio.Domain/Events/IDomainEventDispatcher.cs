namespace Portfolio.Domain.Events;

/// <summary>
/// Dispatches domain events to their respective handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent evt, CancellationToken ct = default) where TEvent : IDomainEvent;
}