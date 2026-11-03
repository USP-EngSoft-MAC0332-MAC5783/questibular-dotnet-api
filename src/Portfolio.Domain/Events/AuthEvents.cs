namespace Portfolio.Domain.Events.AuthEvents;

public sealed record UserRegisteredEvent(string UserId, string Email, string Name, DateTime OccurredAt) : IDomainEvent;
public sealed record UserLoggedInEvent(string UserId, string? Ip, bool RequiresTwoFactor, DateTime OccurredAt) : IDomainEvent;
public sealed record UserLoggedOutEvent(string UserId, string? Ip, DateTime OccurredAt) : IDomainEvent;
public sealed record AccountLockedEvent(string UserId, DateTime LockoutEnd, string? Ip, DateTime OccurredAt) : IDomainEvent;
public sealed record RefreshTokenRotatedEvent(string UserId, string OldTokenId, string NewTokenId, DateTime OccurredAt) : IDomainEvent;
public sealed record RefreshTokenReusedEvent(string UserId, string TokenId, string? Ip, DateTime OccurredAt) : IDomainEvent;
public sealed record TwoFactorEnabledEvent(string UserId, DateTime OccurredAt) : IDomainEvent;
public sealed record TwoFactorDisabledEvent(string UserId, DateTime OccurredAt) : IDomainEvent;