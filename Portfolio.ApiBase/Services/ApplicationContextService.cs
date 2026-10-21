namespace Portfolio.ApiBase.Services;

public class ApplicationContextService : IApplicationContextService
{
    private readonly ConcurrentDictionary<string, object> _contextItems = new();

    // Implement the interface properties
    public string? CurrentUserId => GetValue<string?>("CurrentUserId");
    public string? RequestId => GetValue<string?>("RequestId");

    public void SetValue(string key, object value)
    {
        _contextItems[key] = value;
    }

    public T GetValue<T>(string key)
    {
        if (_contextItems.TryGetValue(key, out var value) && value is T typedValue)
        {
            return ReturnOrThrowIfNull(key, typedValue);
        }

        throw new KeyNotFoundException($"Key '{key}' not found in application context.");
    }

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

    // Fixed helper method (removed the syntax error)
    private static T ReturnOrThrowIfNull<T>(string key, T? value)
    {
        if (value is not null)
            return value;

        throw new InvalidOperationException(
            $"ApplicationContextService - Required value for '{key}' is null. Ensure it's set before accessing."
        );
    }
}