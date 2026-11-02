namespace Portfolio.Api.Features.Auth.Handlers;

public sealed partial class SendWelcomeEmailHandler : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly IEmailSender _email;
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(IEmailSender email, ILogger<SendWelcomeEmailHandler> logger)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(UserRegisteredEvent evt, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(evt);

        await _email.SendAsync(
            new EmailMessage(
                To: evt.Email,
                Subject: "Bem-vindo ao Portfolio",
                Body: $"Olá {evt.Name}, sua conta foi criada com sucesso.",
                IsHtml: true),
            ct);

        LogEnviado(evt.UserId, evt.Email);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Email de boas-vindas enviado para {UserId} ({Email})")]
    private partial void LogEnviado(string userId, string email);
}