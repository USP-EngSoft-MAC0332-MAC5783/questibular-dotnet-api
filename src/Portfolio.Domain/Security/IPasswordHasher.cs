namespace Portfolio.Domain.Security;

/// <summary>
/// Defines the contract for hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password.
    /// </summary>
    string Hash(string plainPassword);

    /// <summary>
    /// Verifies a provided password against a stored hash.
    /// </summary>
    PasswordVerification Verify(string hashedPassword, string providedPassword);
}

/// <summary>
/// Represents the result of a password verification operation.
/// </summary>
public enum PasswordVerification
{
    Failed,
    Success,
    SuccessRehashNeeded
}