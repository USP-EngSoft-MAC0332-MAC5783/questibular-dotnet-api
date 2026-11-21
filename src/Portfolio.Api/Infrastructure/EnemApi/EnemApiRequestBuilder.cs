namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// EnemApiRequestBuilder
// Generic builder that construct HttpRequestMessage from templates
// defined in .env. It supports:
//   • HTTP (GET/POST/…);
//   • replace placeholders "%key%" in path;
//   • query string typed (bool, int, string);
//   • custom headers;
//   • JSON body;
//   • individual timeout per request.
// =============================================================================
public sealed class EnemApiRequestBuilder
{
    private HttpMethod _method = HttpMethod.Get;
    private string? _pathTemplate;
    private readonly Dictionary<string, object?> _pathValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string?> _query = new();
    private readonly Dictionary<string, string> _headers = new();
    private object? _body;
    private TimeSpan? _timeout;

    private EnemApiRequestBuilder() { }

    public static EnemApiRequestBuilder Create() => new();

    public EnemApiRequestBuilder WithMethod(HttpMethod method)
    {
        _method = method ?? throw new ArgumentNullException(nameof(method));
        return this;
    }

    public EnemApiRequestBuilder WithGet()  => WithMethod(HttpMethod.Get);
    public EnemApiRequestBuilder WithPost() => WithMethod(HttpMethod.Post);

    /// <summary>Defines the path template and replaces the "%key%" placeholders.</summary>
    public EnemApiRequestBuilder WithPath(string template, params (string Key, object? Value)[] values)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Invalid Path template.", nameof(template));

        _pathTemplate = template;
        foreach (var (k, v) in values) _pathValues[k] = v;
        return this;
    }

    /// <summary>Defines a query parameter. Ignores nulls/empty values.</summary>
    public EnemApiRequestBuilder WithQuery(string key, object? value)
    {
        if (value is null) return this;
        if (value is string s && string.IsNullOrWhiteSpace(s)) return this;

        _query[key] = value switch
        {
            bool b => b ? "true" : "false",
            _ => value.ToString()
        };
        return this;
    }

    public EnemApiRequestBuilder WithHeader(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public EnemApiRequestBuilder WithJsonBody(object body)
    {
        _body = body ?? throw new ArgumentNullException(nameof(body));
        return this;
    }

    public EnemApiRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public TimeSpan? Timeout => _timeout;

    public HttpRequestMessage Build()
    {
        if (_pathTemplate is null)
            throw new InvalidOperationException("Path not configured.");

        var path = _pathTemplate;
        foreach (var (key, value) in _pathValues)
        {
            var escaped = Uri.EscapeDataString(value?.ToString() ?? string.Empty);
            path = path.Replace($"%{key}%", escaped, StringComparison.OrdinalIgnoreCase);
        }

        if (_query.Count > 0)
        {
            var qs = string.Join("&", _query.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
            path = path.Contains('?') ? $"{path}&{qs}" : $"{path}?{qs}";
        }

        var request = new HttpRequestMessage(_method, path);
        foreach (var (k, v) in _headers)
            request.Headers.TryAddWithoutValidation(k, v);

        if (_body is not null)
            request.Content = JsonContent.Create(_body, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return request;
    }
}