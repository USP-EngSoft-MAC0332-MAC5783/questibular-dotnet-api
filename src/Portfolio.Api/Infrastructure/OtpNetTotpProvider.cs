namespace Portfolio.Api.Infrastructure;

public sealed class OtpNetTotpProvider : ITotpProvider
{
    private const int SecretLengthBytes = 20;
    private const int CodeDigits = 6;
    private const int StepSeconds = 30;

    public (string Secret, string OtpAuthUri) GenerateSecret(string issuer, string userEmail)
    {
        var secret = Base32Encoding.ToString(RandomNumberGenerator.GetBytes(SecretLengthBytes));
        var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}" +
                  $"?secret={secret}&issuer={Uri.EscapeDataString(issuer)}" +
                  $"&algorithm=SHA1&digits={CodeDigits}&period={StepSeconds}";
        return (secret, uri);
    }

    public bool ValidateCode(string secret, string code, int window = 1)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code)) return false;
        var totp = new Totp(Base32Encoding.ToBytes(secret), step: StepSeconds, totpSize: CodeDigits);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: window, future: window));
    }

    public IReadOnlyList<string> GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(5));
            codes.Add($"{raw[..5]}-{raw[5..]}");
        }
        return codes;
    }

    public bool TryConsumeRecoveryCode(List<string> hashedCodes, string candidate)
    {
        var candidateHash = HashRecoveryCode(candidate);
        for (var i = 0; i < hashedCodes.Count; i++)
        {
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(hashedCodes[i]),
                    Encoding.UTF8.GetBytes(candidateHash)))
            {
                hashedCodes.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public string HashRecoveryCode(string code)
        => Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(code.Replace("-", "").ToUpperInvariant())));
}