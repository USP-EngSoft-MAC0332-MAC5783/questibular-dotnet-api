namespace Portfolio.Domain.Security;

/// <summary>
/// Provides an abstraction for retrieving the current date and time.
/// Useful for unit testing time-dependent business logic.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}