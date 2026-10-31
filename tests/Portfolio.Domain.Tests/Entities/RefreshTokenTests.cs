namespace Portfolio.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_Should_Throw_When_UserId_Is_Empty()
    {
        var act = () => RefreshToken.Create("", "hash", DateTime.UtcNow.AddDays(1), "127.0.0.1", DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*User ID*");
    }

    [Fact]
    public void Create_Should_Throw_When_TokenHash_Is_Empty()
    {
        var act = () => RefreshToken.Create("user1", "", DateTime.UtcNow.AddDays(1), "127.0.0.1", DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*Token hash*");
    }

    [Fact]
    public void Create_Should_Throw_When_Expiration_Is_In_The_Past()
    {
        var now = DateTime.UtcNow;
        var act = () => RefreshToken.Create("user1", "hash", now.AddDays(-1), "127.0.0.1", now);
        act.Should().Throw<ArgumentException>().WithMessage("*Expiration date*");
    }

    [Fact]
    public void Create_Should_Return_Valid_Token()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create("user1", "hash", now.AddDays(1), "127.0.0.1", now);

        token.Id.Should().NotBeNullOrEmpty();
        token.UserId.Should().Be("user1");
        token.TokenHash.Should().Be("hash");
        token.ExpiresAt.Should().BeAfter(now);
        token.CreatedByIp.Should().Be("127.0.0.1");
        token.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void IsExpiredAt_Should_Work_Correctly()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create("user1", "hash", now.AddDays(1), "127.0.0.1", now);

        token.IsExpiredAt(now).Should().BeFalse();
        token.IsExpiredAt(now.AddDays(2)).Should().BeTrue();
    }

    [Fact]
    public void IsActiveAt_Should_Work_Correctly()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create("user1", "hash", now.AddDays(1), "127.0.0.1", now);

        token.IsActiveAt(now).Should().BeTrue();
        
        token.Revoke(now, "127.0.0.1");
        token.IsActiveAt(now).Should().BeFalse();
    }

    [Fact]
    public void Revoke_Should_Set_Fields_But_Not_Overwrite_If_Already_Revoked()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create("user1", "hash", now.AddDays(1), "127.0.0.1", now);

        // Primeira revogação
        token.Revoke(now, "192.168.0.1", "new-hash");
        token.RevokedAt.Should().Be(now);
        token.RevokedByIp.Should().Be("192.168.0.1");
        token.ReplacedByTokenHash.Should().Be("new-hash");

        // Tentativa de revogar novamente (deve ser ignorada)
        token.Revoke(now.AddHours(1), "outro-ip");
        token.RevokedAt.Should().Be(now); 
        token.RevokedByIp.Should().Be("192.168.0.1");
    }
}