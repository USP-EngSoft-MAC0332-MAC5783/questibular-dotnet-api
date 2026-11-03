namespace Portfolio.Api.Features.Auth.Handlers;

public sealed partial class NotifyAccountLockedHandler : IDomainEventHandler<AccountLockedEvent>
{
    private readonly ILogger<NotifyAccountLockedHandler> _logger;

    public NotifyAccountLockedHandler(ILogger<NotifyAccountLockedHandler> logger)
        => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task HandleAsync(AccountLockedEvent evt, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(evt);
        //TODO: Implementar envio de email para o usuário notificando que a conta foi bloqueada
        LogBloqueio(evt.UserId, evt.LockoutEnd);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Conta bloqueada: {UserId} até {LockoutEnd:O}")]
    private partial void LogBloqueio(string userId, DateTime lockoutEnd);
}