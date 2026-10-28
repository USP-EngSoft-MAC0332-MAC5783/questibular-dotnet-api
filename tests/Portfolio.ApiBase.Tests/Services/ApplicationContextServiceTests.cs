namespace Portfolio.ApiBase.Tests.Services;

public class ApplicationContextServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ApplicationContextService _sut;

    public ApplicationContextServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sut = new ApplicationContextService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void CurrentUserId_ShouldReturnClaim_WhenUserIsAuthenticated()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user_123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _sut.CurrentUserId;

        // Assert
        result.Should().Be("user_123");
    }

    [Fact]
    public void CurrentUserId_ShouldThrow_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

        // Act
        var action = () => _sut.CurrentUserId;

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}