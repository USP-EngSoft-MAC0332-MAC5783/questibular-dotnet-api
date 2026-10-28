namespace Portfolio.ApiBase.Tests.Configuration;

public class ConfiguratorsTests
{
    private readonly Mock<ILogger> _loggerMock = new();
    
    private IConfiguration GetConfiguration(Dictionary<string, string> inMemoryConfig)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig!).Build();
    }

    [Fact]
    public void CorsConfigurator_ShouldLoadSettings()
    {
        // Arrange
        var config = GetConfiguration(new Dictionary<string, string>
        {
            { "Cors:AllowedOrigins:0", "https://localhost" },
            { "Cors:AllowCredentials", "true" }
        });

        // Act
        var settings = CorsPolicyConfigurator.LoadCorsSettings(config, _loggerMock.Object);

        // Assert
        settings.AllowedOrigins.Should().Contain("https://localhost");
        settings.AllowCredentials.Should().BeTrue();
    }

    [Fact]
    public void HttpsConfigurator_ShouldLoadSettings()
    {
        // Arrange
        var config = GetConfiguration(new Dictionary<string, string>
        {
            { "Https:CertificatePemPath", "/app/certs/cert.pem" },
            { "Https:CertificateKeyPath", "/app/certs/key.pem" }
        });

        // Act
        var settings = HttpsConfigurator.LoadHttpsSettings(config, _loggerMock.Object);

        // Assert
        settings.CertificatePemPath.Should().Be("/app/certs/cert.pem");
        settings.CertificateKeyPath.Should().Be("/app/certs/key.pem");
    }
}