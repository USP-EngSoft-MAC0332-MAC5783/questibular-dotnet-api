namespace Portfolio.Domain.Security;

/// <summary>
/// Defines the contract for encrypting and decrypting sensitive data (e.g., 2FA secrets).
/// </summary>
public interface ISecretProtector
{
    /// <summary>
    /// Encrypts a plaintext string.
    /// </summary>
    string Protect(string plaintext);

    /// <summary>
    /// Decrypts a ciphertext string.
    /// </summary>
    string Unprotect(string ciphertext);
}