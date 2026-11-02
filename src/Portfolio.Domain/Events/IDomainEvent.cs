namespace Portfolio.Domain.Events;

/// <summary>
/// Marker interface for domain events. Represents a significant business event
/// that has occurred within the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The UTC date and time when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}