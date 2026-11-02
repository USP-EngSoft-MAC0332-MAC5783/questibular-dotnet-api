namespace Portfolio.ApiBase.Tests.Controllers;

public class BaseApiControllerTests
{
    // Controller dummy
    private class TestController : BaseApiController<TestController>
    {
        public TestController(IApplicationContextService context) 
            : base(NullLogger<TestController>.Instance, context) { }

        public IActionResult TestBadRequest(string msg) => BadRequestWithLog(msg);
        public IActionResult TestNotFound(string msg) => NotFoundWithLog(msg);
        public string? TestCurrentUserId() => GetCurrentUserId();
    }

    [Fact]
    public void BadRequestWithLog_Should_Return_BadRequestObjectResult_With_Error_Payload()
    {
        // Arrange
        var controller = new TestController(Mock.Of<IApplicationContextService>());

        // Act
        var result = controller.TestBadRequest("Invalid data") as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().BeEquivalentTo(new { Error = "Invalid data" });
    }

    [Fact]
    public void NotFoundWithLog_Should_Return_NotFoundObjectResult()
    {
        // Arrange
        var controller = new TestController(Mock.Of<IApplicationContextService>());

        // Act
        var result = controller.TestNotFound("User not found") as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(404);
        result.Value.Should().BeEquivalentTo(new { Error = "User not found" });
    }

    [Fact]
    public void GetCurrentUserId_Should_Read_From_AppContext()
    {
        // Arrange
        var contextMock = new Mock<IApplicationContextService>();
        string? expectedUserId = "user-abc";
        
        contextMock.Setup(c => c.TryGetValue<string?>("CurrentUserId", out expectedUserId))
                   .Returns(true);

        var controller = new TestController(contextMock.Object);

        // Act
        var result = controller.TestCurrentUserId();

        // Assert
        result.Should().Be("user-abc");
    }
}