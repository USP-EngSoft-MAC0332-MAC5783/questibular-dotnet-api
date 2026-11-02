namespace Portfolio.Domain.Security;

/// <summary>
/// Defines the contract for generating authentication and security tokens.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a JWT access token for an authenticated user.
    /// </summary>
    AccessToken GenerateAccessToken(Entities.User user);

    /// <summary>
    /// Generates a new refresh token for a user.
    /// </summary>
    RefreshTokenResult GenerateRefreshToken(string userId, string? createdByIp = null);

    /// <summary>
    /// Computes a cryptographic hash of a raw refresh token.
    /// </summary>
    string HashRefreshToken(string rawToken);

    /// <summary>
    /// Generates a temporary token used during the 2FA pending state.
    /// </summary>
    string GeneratePendingTwoFactorToken(string userId);

    /// <summary>
    /// Extracts the user identifier from a pending 2FA token.
    /// </summary>
    bool TryGetUserIdFromPendingToken(string token, out string userId);
}

/// <summary>
/// Represents a generated access token and its expiration date.
/// </summary>
public sealed record AccessToken(string Token, DateTime ExpiresAt);

/// <summary>
/// Represents the result of generating a refresh token, including the entity and the raw token.
/// </summary>
public sealed record RefreshTokenResult(Entities.RefreshToken Entity, string RawToken);