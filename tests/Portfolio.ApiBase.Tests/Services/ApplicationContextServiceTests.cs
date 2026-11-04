namespace Portfolio.ApiBase.Tests.Services;

public class ApplicationContextServiceTests
{
    private readonly HttpContextAccessor _accessor;
    private readonly ApplicationContextService _service;

    public ApplicationContextServiceTests()
    {
        _accessor = new HttpContextAccessor();
        _service = new ApplicationContextService(_accessor);
    }

    [Fact]
    public void SetValue_And_GetValue_Should_Work()
    {
        // Arrange
        var key = "MyKey";
        var value = "MyValue";

        // Act
        _service.SetValue(key, value);
        var result = _service.GetValue<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void GetValue_Should_Throw_KeyNotFoundException_When_Key_Missing()
    {
        // Act
        var act = () => _service.GetValue<string>("MissingKey");

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void TryGetValue_Should_Return_True_And_Value_When_Exists()
    {
        // Arrange
        _service.SetValue("Age", 25);

        // Act
        var found = _service.TryGetValue<int>("Age", out var result);

        // Assert
        found.Should().BeTrue();
        result.Should().Be(25);
    }

    [Fact]
    public void TryGetValue_Should_Return_False_When_Key_Does_Not_Exist()
    {
        // Act
        var found = _service.TryGetValue<string>("Missing", out var result);

        // Assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void CurrentUserId_Should_Return_Claim_Value_When_Authenticated()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _accessor.HttpContext = context;

        // Act
        var userId = _service.CurrentUserId;

        // Assert
        userId.Should().Be("user-123");
    }

    [Fact]
    public void CurrentUserId_Should_Throw_When_Not_Authenticated()
    {
        // Arrange
        _accessor.HttpContext = new DefaultHttpContext();

        // Act
        var act = () => _service.CurrentUserId;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*User is not authenticated*");
    }

    [Fact]
    public void RequestId_Should_Return_TraceIdentifier()
    {
        // Arrange
        var context = new DefaultHttpContext { TraceIdentifier = "trace-999" };
        _accessor.HttpContext = context;

        // Act
        var requestId = _service.RequestId;

        // Assert
        requestId.Should().Be("trace-999");
    }
}