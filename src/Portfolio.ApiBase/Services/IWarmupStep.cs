namespace Portfolio.ApiBase.Services;

/// <summary>
/// Represents an individual initialization step executed asynchronously during app startup.
/// </summary>
public interface IWarmupStep
{
    /// <summary>
    /// Step name (used for logs).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the warmup logic.
    /// </summary>
    Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}