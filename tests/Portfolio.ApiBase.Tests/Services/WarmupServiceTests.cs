namespace Portfolio.ApiBase.Tests.Services;

public class WarmupServiceTests
{
    [Fact]
    public async Task StartAsync_Should_Execute_All_Warmup_Steps()
    {
        // Arrange
        var step1Mock = new Mock<IWarmupStep>();
        step1Mock.Setup(s => s.Name).Returns("Step 1");
        
        var step2Mock = new Mock<IWarmupStep>();
        step2Mock.Setup(s => s.Name).Returns("Step 2");

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var warmupService = new WarmupService(
            NullLogger<WarmupService>.Instance,
            serviceProvider,
            [step1Mock.Object, step2Mock.Object]
        );

        // Act
        await warmupService.StartAsync(CancellationToken.None);

        // Assert
        step1Mock.Verify(s => s.ExecuteAsync(It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()), Times.Once);
        step2Mock.Verify(s => s.ExecuteAsync(It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}