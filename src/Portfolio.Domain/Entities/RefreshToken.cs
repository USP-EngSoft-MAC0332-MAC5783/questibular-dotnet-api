namespace Portfolio.Domain.Entities;

/// <summary>
/// Represents a refresh token used to maintain a user's authenticated session.
/// </summary>
public sealed class RefreshToken : EntityMetadata
{
    /// <summary>
    /// The identifier of the user who owns this token.
    /// </summary>
    public string UserId { get; private set; } = default!;

    /// <summary>
    /// The cryptographic hash of the raw refresh token. Never store the raw token.
    /// </summary>
    public string TokenHash { get; private set; } = default!;

    /// <summary>
    /// The UTC date and time when this token expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// The UTC date and time when this token was revoked (if applicable).
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// The hash of the new token that replaced this one during rotation (if applicable).
    /// </summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>
    /// The IP address from which the token was created.
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// The IP address from which the token was revoked.
    /// </summary>
    public string? RevokedByIp { get; private set; }

    private RefreshToken() { }
    
    private RefreshToken(string userId, string tokenHash, DateTime expiresAt, string? createdByIp)
    {
        Id = Guid.NewGuid().ToString("N");
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
    }

    /// <summary>
    /// Creates a new RefreshToken instance for a new authentication session.
    /// Applies strict business rules and validations before the token is issued.
    /// </summary>
    /// <param name="userId">The identifier of the user who owns the token.</param>
    /// <param name="tokenHash">The cryptographic hash of the raw refresh token.</param>
    /// <param name="expiresAt">The UTC date and time when the token should expire.</param>
    /// <param name="createdByIp">The IP address from which the token is being created.</param>
    /// <param name="now">The current UTC date and time, used to validate the expiration date.</param>
    /// <returns>A newly created <see cref="RefreshToken"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when user ID or token hash is empty, or when the expiration date is in the past.</exception>
    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAt, string? createdByIp, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));

        if (expiresAt <= now)
            throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));

        return new RefreshToken(userId, tokenHash, expiresAt, createdByIp);
    }

    /// <summary>
    /// Reconstitutes an existing RefreshToken entity from persistence (e.g., database).
    /// This method is intended to be used exclusively by the Infrastructure layer to rehydrate 
    /// an object without re-executing creation validations.
    /// </summary>
    /// <param name="id">The unique identifier of the token.</param>
    /// <param name="userId">The identifier of the user who owns the token.</param>
    /// <param name="tokenHash">The cryptographic hash of the raw refresh token.</param>
    /// <param name="expiresAt">The UTC date and time when the token expires.</param>
    /// <param name="revokedAt">The UTC date and time when the token was revoked, if applicable.</param>
    /// <param name="replacedByTokenHash">The hash of the token that replaced this one, if applicable.</param>
    /// <param name="createdByIp">The IP address from which the token was created.</param>
    /// <param name="revokedByIp">The IP address from which the token was revoked, if applicable.</param>
    /// <param name="createdAt">The UTC date and time when the token record was originally created.</param>
    /// <returns>The reconstituted <see cref="RefreshToken"/> instance.</returns>
    public static RefreshToken Reconstitute(
        string id, string userId, string tokenHash, DateTime expiresAt, 
        DateTime? revokedAt, string? replacedByTokenHash, string? createdByIp, 
        string? revokedByIp, DateTime createdAt)
    {
        var token = new RefreshToken
        {
            Id = id,
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt,
            ReplacedByTokenHash = replacedByTokenHash,
            CreatedByIp = createdByIp,
            RevokedByIp = revokedByIp,
            CreatedAt = createdAt
        };
        return token;
    }

    /// <summary>
    /// Determines whether the token has expired based on the provided current time.
    /// </summary>
    /// <param name="now">The current UTC date and time.</param>
    /// <returns>True if expired; otherwise, false.</returns>
    public bool IsExpiredAt(DateTime now) => ExpiresAt <= now;

    /// <summary>
    /// Determines whether the token is active (not revoked and not expired) at the given time.
    /// </summary>
    /// <param name="now">The current UTC date and time.</param>
    /// <returns>True if active; otherwise, false.</returns>
    public bool IsActiveAt(DateTime now) => RevokedAt is null && !IsExpiredAt(now);

    /// <summary>
    /// Revokes the current token and records the IP address and reason.
    /// </summary>
    /// <param name="revokedAt">The UTC date and time of revocation.</param>
    /// <param name="revokedByIp">The IP address from which the token was revoked.</param>
    /// <param name="replacedByTokenHash">The hash of the token replacing this one, if any.</param>
    public void Revoke(DateTime revokedAt, string? revokedByIp, string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null) return;

        RevokedAt = revokedAt;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}