namespace Portfolio.Api.Features.Users.Contracts;

public sealed record UserResponse(
    string Id,
    string Email,
    string Name,
    IReadOnlyList<string> Roles);