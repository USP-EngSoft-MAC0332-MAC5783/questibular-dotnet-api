namespace Portfolio.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_Should_Throw_When_Email_Is_Empty()
    {
        var act = () => User.Create("", "passwordHash", DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*Email*");
    }

    [Fact]
    public void Create_Should_Return_Valid_User_With_Default_Role()
    {
        var user = User.Create("test@test.com", "hash", DateTime.UtcNow);

        user.Id.Should().NotBeNullOrEmpty();
        user.Email.Should().Be("test@test.com");
        user.Roles.Should().ContainSingle().Which.Should().Be("User");
    }

    [Fact]
    public void RegisterFailedAccess_Should_Lock_User_When_Threshold_Is_Reached()
    {
        var user = User.Create("test@test.com", "hash", DateTime.UtcNow);
        var now = DateTime.UtcNow;

        user.RegisterFailedAccess(2, TimeSpan.FromMinutes(5), now);
        user.AccessFailedCount.Should().Be(1);
        user.IsLockedOutAt(now).Should().BeFalse();

        user.RegisterFailedAccess(2, TimeSpan.FromMinutes(5), now);
        user.AccessFailedCount.Should().Be(2);
        user.IsLockedOutAt(now).Should().BeTrue();
    }

    [Fact]
    public void RegisterSuccessfulLogin_Should_Reset_Counters_And_Set_LastLogin()
    {
        var user = User.Create("test@test.com", "hash", DateTime.UtcNow);
        var now = DateTime.UtcNow;
        
        user.RegisterFailedAccess(2, TimeSpan.FromMinutes(5), now);
        user.RegisterFailedAccess(2, TimeSpan.FromMinutes(5), now);
        
        user.RegisterSuccessfulLogin(now);

        user.AccessFailedCount.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.LastLoginAt.Should().Be(now);
    }

    [Fact]
    public void TwoFactor_Workflow_Should_Work_Correctly()
    {
        var user = User.Create("test@test.com", "hash", DateTime.UtcNow);
        var now = DateTime.UtcNow;

        // Setup
        user.SetupTwoFactor("secret");
        user.TwoFactorSecret.Should().Be("secret");
        user.TwoFactorEnabled.Should().BeFalse();

        // Enable
        user.EnableTwoFactor(new List<string> { "code1", "code2" }, now);
        user.TwoFactorEnabled.Should().BeTrue();
        user.TwoFactorEnabledAt.Should().Be(now);
        user.TwoFactorRecoveryCodes.Should().HaveCount(2);

        // Use code
        user.UseRecoveryCode("code1").Should().BeTrue();
        user.UseRecoveryCode("invalid").Should().BeFalse();
        user.TwoFactorRecoveryCodes.Should().HaveCount(1);

        // Disable
        user.DisableTwoFactor();
        user.TwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();
        user.TwoFactorRecoveryCodes.Should().BeEmpty();
    }

    [Fact]
    public void SetupTwoFactor_Should_Throw_If_Already_Enabled()
    {
        var user = User.Create("test@test.com", "hash", DateTime.UtcNow);
        user.EnableTwoFactor(new List<string>(), DateTime.UtcNow);

        var act = () => user.SetupTwoFactor("secret");
        act.Should().Throw<InvalidOperationException>().WithMessage("*já está habilitado*");
    }
}