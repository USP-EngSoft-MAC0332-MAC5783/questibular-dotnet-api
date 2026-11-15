namespace Portfolio.Api.Tests.Infrastructure;

// =============================================================================
// FakeHttpMessageHandler
// Intercepts HttpClient requests for testing (Mocked Tests).
// =============================================================================
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;
    
    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(_responseFactory(request));
    }
}