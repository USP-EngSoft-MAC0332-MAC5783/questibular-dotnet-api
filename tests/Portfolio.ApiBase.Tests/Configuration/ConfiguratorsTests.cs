namespace Portfolio.ApiBase.Tests.Configuration;

public class ConfigurationTests
{
    [Fact]
    public void CorsConfigurator_Should_Override_Origins_From_Env_Var()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { CorsPolicyConfigurator.ENV_CORS_ALLOWED_ORIGINS, "https://site1.com;https://site2.com" },
                { CorsPolicyConfigurator.ENV_ALLOW_CREDENTIALS, "true" }
            })
            .Build();

        // Act
        var settings = CorsPolicyConfigurator.LoadCorsSettings(config, NullLogger.Instance);

        // Assert
        settings.AllowedOrigins.Should().HaveCount(2).And.Contain("https://site1.com");
        settings.AllowCredentials.Should().BeTrue();
    }

    [Fact]
    public void CorsConfigurator_Should_Ignore_AllowCredentials_When_Origins_Are_Empty()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { CorsPolicyConfigurator.ENV_ALLOW_CREDENTIALS, "true" }
                // Sem origens configuradas
            })
            .Build();

        // Act
        var policy = CorsPolicyConfigurator.Configure(config, NullLogger.Instance);

        // Assert
        // O código de produção intercepta o erro do .NET e ignora o AllowCredentials.
        // Portanto, não deve lançar exceção e a flag deve continuar falsa.
        policy.Should().NotBeNull();
        policy.SupportsCredentials.Should().BeFalse("porque não há origens específicas configuradas");
    }

    [Fact]
    public void HttpsConfigurator_Should_Override_Certificate_Path_From_Env_Var()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { HttpsConfigurator.ENV_CERTIFICATE_PEM_PATH, "/certs/my-cert.pem" }
            })
            .Build();

        // Act
        var settings = HttpsConfigurator.LoadHttpsSettings(config, NullLogger.Instance);

        // Assert
        settings.CertificatePemPath.Should().Be("/certs/my-cert.pem");
    }
}