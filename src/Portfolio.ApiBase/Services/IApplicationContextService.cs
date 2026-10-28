namespace Portfolio.ApiBase.Services;

public interface IApplicationContextService
{
    /// <summary>
    /// Sets a value in the application context.
    /// </summary>
    /// <param name="key">The key to identify the value.</param>
    /// <param name="value">The value to store.</param>
    void SetValue(string key, object value);

    /// <summary>
    /// Retrieves a value from the application context.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The stored value of type T.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the value is null.</exception>
    T GetValue<T>(string key);

    bool TryGetValue<T>(string key, out T value);

    string? CurrentUserId { get; }
    string? RequestId { get; }
}