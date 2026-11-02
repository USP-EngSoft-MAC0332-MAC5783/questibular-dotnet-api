namespace Portfolio.Domain.Events.AuthEvents;

public sealed record UserRegisteredEvent(string UserId, string Email, string Name, DateTime OccurredAt) : IDomainEvent;