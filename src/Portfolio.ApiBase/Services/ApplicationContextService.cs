namespace Portfolio.ApiBase.Services;

/// <summary>
/// Scoped service that provides access to the current application/request context.
/// Exposes the authenticated user's identifier, the current trace (request) identifier,
/// and a per-request key/value store for sharing data across the pipeline.
/// </summary>
/// <remarks>
/// This service is intended to be registered with a scoped lifetime, since it relies on
/// <see cref="IHttpContextAccessor"/> and stores data in-memory for the duration of a request.
/// The internal dictionary is thread-safe for concurrent reads/writes within the same scope.
/// </remarks>
public class ApplicationContextService : IApplicationContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // Per-request in-memory store for arbitrary values shared across the request pipeline.
    // Uses ConcurrentDictionary for thread safety in case of parallel access within the same scope.
    private readonly ConcurrentDictionary<string, object> _contextItems = new();

    /// <summary>
    /// Gets the identifier of the currently authenticated user.
    /// </summary>
    /// <value>
    /// The value of the <see cref="ClaimTypes.NameIdentifier"/> claim on the current HTTP user.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there is no authenticated user (i.e. the claim is missing or empty).
    /// </exception>
    public string CurrentUserId 
    {
        get
        {
            // Read Claims from HTTP user
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("User is not authenticated.");
                
            return userId;
        }
    }

    /// <summary>
    /// Gets the identifier associated with the current request (ASP.NET Core's trace identifier).
    /// </summary>
    /// <value>
    /// The current <see cref="HttpContext.TraceIdentifier"/>, or an empty string if
    /// no HTTP context is available (e.g. during startup or background work).
    /// </value>
    public string? RequestId => _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationContextService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Accessor used to read the current <see cref="HttpContext"/>. Injected via DI.
    /// </param>

    public ApplicationContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Stores (or overwrites) a value under the specified key in the current request's context.
    /// </summary>
    /// <param name="key">The key under which the value is stored. Must be non-null.</param>
    /// <param name="value">The value to store. Must be non-null.</param>
    public void SetValue(string key, object value)
    {
        _contextItems[key] = value;
    }

    /// <summary>
    /// Retrieves a strongly typed value from the current context.
    /// </summary>
    /// <typeparam name="T">The expected type of the stored value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <returns>The stored value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the key does not exist or the stored value is not of type <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the stored value is <c>null</c> (via <see cref="ReturnOrThrowIfNull{T}"/>).
    /// </exception>
    public T GetValue<T>(string key)
    {
        if (_contextItems.TryGetValue(key, out var value) && value is T typedValue)
        {
            return ReturnOrThrowIfNull(key, typedValue);
        }

        throw new KeyNotFoundException($"Key '{key}' not found in application context.");
    }

    /// <summary>
    /// Attempts to retrieve a strongly typed value from the current context without throwing.
    /// </summary>
    /// <typeparam name="T">The expected type of the stored value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">
    /// When this method returns <c>true</c>, contains the stored value; otherwise, <c>default</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key exists and the stored value is of type <typeparamref name="T"/>; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetValue<T>(string key, out T value)
    {
        if (_contextItems.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>
    /// Returns <paramref name="value"/> when non-null, otherwise throws a descriptive exception.
    /// </summary>
    /// <typeparam name="T">The type of the value being validated.</typeparam>
    /// <param name="key">The context key used in the error message for diagnostics.</param>
    /// <param name="value">The value to validate and return.</param>
    /// <returns>The non-null <paramref name="value"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="value"/> is <c>null</c>, indicating a value was registered
    /// under <paramref name="key"/> but is unexpectedly null.
    /// </exception>
    private static T ReturnOrThrowIfNull<T>(string key, T? value)
    {
        if (value is not null)
            return value;

        throw new InvalidOperationException(
            $"ApplicationContextService - Required value for '{key}' is null. Ensure it's set before accessing."
        );
    }
}