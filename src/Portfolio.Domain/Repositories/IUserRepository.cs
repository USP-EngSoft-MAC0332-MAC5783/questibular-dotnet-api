namespace Portfolio.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Entities.User"/> instances.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<Entities.User?> GetByEmailAsync(string email, CancellationToken ct);

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<Entities.User?> GetByIdAsync(string id, CancellationToken ct);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    Task AddAsync(Entities.User user, CancellationToken ct);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    Task UpdateAsync(Entities.User user, CancellationToken ct);
}