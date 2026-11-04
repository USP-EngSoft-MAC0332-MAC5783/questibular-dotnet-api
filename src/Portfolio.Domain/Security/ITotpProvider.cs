namespace Portfolio.Domain.Security;

/// <summary>
/// Defines the contract for Time-based One-Time Password (TOTP) operations.
/// </summary>
public interface ITotpProvider
{
    /// <summary>
    /// Generates a new TOTP secret and its corresponding OTP Auth URI (for QR Code generation).
    /// </summary>
    (string Secret, string OtpAuthUri) GenerateSecret(string issuer, string userEmail);

    /// <summary>
    /// Validates a TOTP code against a secret, allowing for a configurable time window.
    /// </summary>
    bool ValidateCode(string secret, string code, int window = 1);

    /// <summary>
    /// Generates a list of recovery codes for 2FA.
    /// </summary>
    IReadOnlyList<string> GenerateRecoveryCodes(int count = 10);

    /// <summary>
    /// Attempts to consume (validate and remove) a recovery code from a list of hashed codes.
    /// </summary>
    bool TryConsumeRecoveryCode(List<string> hashedCodes, string candidate);

    /// <summary>
    /// Computes a hash of a recovery code.
    /// </summary>
    string HashRecoveryCode(string code);
}