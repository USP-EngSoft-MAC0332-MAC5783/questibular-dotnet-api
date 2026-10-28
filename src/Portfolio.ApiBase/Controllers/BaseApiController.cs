namespace Portfolio.ApiBase.Controllers;

/// <summary>
/// Base controller for all API endpoints, providing shared dependencies and standard response helpers.
/// </summary>
/// <remarks>
/// Using a generic base controller with <typeparamref name="TController"/> ensures that the 
/// injected <see cref="ILogger{TCategoryName}"/> has the correct category name for the derived controller, 
/// improving structured logging context.
/// </remarks>
[ApiController]
public abstract class BaseApiController<TController> : ControllerBase where TController : ControllerBase
{
    protected ILogger<TController> Logger { get; }
    protected IApplicationContextService AppContext { get; }

    // SONAR ignores S6672
    protected BaseApiController(ILogger<TController> logger, IApplicationContextService applicationContextService) // NOSONAR
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        AppContext = applicationContextService ?? throw new ArgumentNullException(nameof(applicationContextService));
    }

    /// <summary>
    /// Retrieves the current user's ID from the application context (e.g., populated by a middleware).
    /// Returns null if the user is not authenticated or the context is missing.
    /// </summary>
    protected string? GetCurrentUserId()
        => AppContext.TryGetValue<string?>("CurrentUserId", out var userId) ? userId : null;

    /// <summary>
    /// Returns a 400 Bad Request response and automatically logs the warning.
    /// Standardizes the error payload format across the API.
    /// </summary>
    protected IActionResult BadRequestWithLog(string message)
    {
        Logger.LogWarning("BadRequest: {Message}", message);
        return BadRequest(new { Error = message });
    }

    /// <summary>
    /// Returns a 404 Not Found response and automatically logs the warning.
    /// Standardizes the error payload format across the API.
    /// </summary>
    protected IActionResult NotFoundWithLog(string message)
    {
        Logger.LogWarning("NotFound: {Message}", message);
        return NotFound(new { Error = message });
    }
}