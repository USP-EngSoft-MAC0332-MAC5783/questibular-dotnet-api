namespace Portfolio.ApiBase.Settings;

public record class HttpsSettings
{
    public string? CertificatePemPath { get; init; }
    public string? CertificateKeyPath { get; init; }
    public string[] SslProtocols { get; init; } = Array.Empty<string>();
    public string ClientCertificateMode { get; init; } = "NoCertificate";

    public HttpsSettings() { }
}