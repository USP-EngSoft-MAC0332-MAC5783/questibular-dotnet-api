namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// EnemApiDispatcher
// Centrilizes the triggers for HTTP requests, handling serialization and 
// error management. Prevents each client from repeating 
// EnsureSuccessStatusCode/NotFound/JSON logic.
// =============================================================================
public interface IEnemApiDispatcher
{
    Task<T?> SendAsync<T>(EnemApiRequestBuilder builder, CancellationToken ct = default);
}

public sealed class EnemApiDispatcher : IEnemApiDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<EnemApiDispatcher> _logger;

    public EnemApiDispatcher(HttpClient http, ILogger<EnemApiDispatcher> logger)
    {
        _http   = http   ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> SendAsync<T>(EnemApiRequestBuilder builder, CancellationToken ct = default)
    {
        using var request = builder.Build();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (builder.Timeout.HasValue) cts.CancelAfter(builder.Timeout.Value);

        using var response = await _http.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
            _logger.LogWarning(
                "ENEM API {Method} {Url} did responded {Status}. Body: {Body}",
                request.Method, request.RequestUri, (int)response.StatusCode, body);

            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cts.Token);
    }
}