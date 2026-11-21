namespace Portfolio.Api.Tests.Infrastructure;

using Microsoft.Extensions.Options;
using Portfolio.Api.Infrastructure.Security;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Security;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateAccessToken_EmitsRoleClaimExpectedByJwtBearerAuthorization()
    {
        var settings = Options.Create(new JwtSettings
        {
            Secret = new string('s', 32),
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
        var generator = new JwtTokenGenerator(settings, new TestClock());
        var user = User.Create(
            "teacher@example.com",
            "Teacher",
            "password-hash",
            UserRole.Teacher,
            DateTime.UtcNow);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            generator.GenerateAccessToken(user).Token);

        Assert.Contains(token.Claims, claim => claim.Type == "role" && claim.Value == "teacher");
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
