namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    Portfolio.Api.Features.Users.Contracts.UserResponse User,
    bool RequiresTwoFactor = false);