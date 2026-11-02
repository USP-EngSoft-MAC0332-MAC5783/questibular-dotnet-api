namespace Portfolio.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Entities.RefreshToken"/> instances.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Retrieves a refresh token by its cryptographic hash.
    /// </summary>
    /// <param name="tokenHash">The hashed refresh token.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The refresh token if found; otherwise, null.</returns>
    Task<Entities.RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct);

    /// <summary>
    /// Adds a new refresh token to the repository.
    /// </summary>
    Task AddAsync(Entities.RefreshToken token, CancellationToken ct);

    /// <summary>
    /// Updates an existing refresh token in the repository.
    /// </summary>
    Task UpdateAsync(Entities.RefreshToken token, CancellationToken ct);

    Task RevokeAllForUserAsync(string userId, string? ip, DateTime now, CancellationToken ct);
}