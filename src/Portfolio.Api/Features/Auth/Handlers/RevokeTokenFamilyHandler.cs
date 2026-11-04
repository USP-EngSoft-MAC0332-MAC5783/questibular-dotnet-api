namespace Portfolio.Api.Features.Auth.Handlers;

public sealed partial class RevokeTokenFamilyHandler : IDomainEventHandler<RefreshTokenReusedEvent>
{
    private readonly IRefreshTokenRepository _tokens;
    private readonly ILogger<RevokeTokenFamilyHandler> _logger;

    public RevokeTokenFamilyHandler(IRefreshTokenRepository tokens, ILogger<RevokeTokenFamilyHandler> logger)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(RefreshTokenReusedEvent evt, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(evt);
        await _tokens.RevokeAllForUserAsync(evt.UserId, evt.Ip, DateTime.UtcNow, ct);
        LogRevogado(evt.UserId, evt.Ip ?? "unknown");
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Família de tokens revogada por reuso. UserId={UserId} Ip={Ip}")]
    private partial void LogRevogado(string userId, string ip);
}