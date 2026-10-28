namespace Portfolio.ApiBase.Services;

/// <summary>
/// Hosted service that executes all registered <see cref="IWarmupStep"/> implementations 
/// during application startup. Used for fail-fast initialization (e.g., DB checks).
/// </summary>
public partial class WarmupService : IHostedService
{
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;
    private readonly IEnumerable<IWarmupStep> _warmupSteps;

    public WarmupService(ILogger<WarmupService> logger, IServiceProvider serviceProvider, IEnumerable<IWarmupStep> warmupSteps)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        _warmupSteps = warmupSteps;
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        LogWarmupStarting(Logger);

        foreach (var step in _warmupSteps)
        {
            LogExecutingStep(Logger, step.Name);

            // Create a new scope for each step to prevent state leaking between scoped dependencies (e.g., DbContext).
            using var scope = ServiceProvider.CreateScope();
            await step.ExecuteAsync(scope.ServiceProvider, cancellationToken);
        }

        LogWarmupCompleted(Logger);
    }

    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting warmup base...")]
    private static partial void LogWarmupStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Running step: {StepName}")]
    private static partial void LogExecutingStep(ILogger logger, string stepName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Warmup base finished.")]
    private static partial void LogWarmupCompleted(ILogger logger);
}