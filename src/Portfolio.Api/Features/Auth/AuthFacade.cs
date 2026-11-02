namespace Portfolio.Api.Features.Auth;

public sealed partial class AuthFacade : IAuthFacade
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly ILogger<AuthFacade> _logger;

    public AuthFacade(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IDomainEventDispatcher eventDispatcher,
        IClock clock,
        ILogger<AuthFacade> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string? ip, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();

        var existingUser = await _userRepository.GetByEmailAsync(email, ct);
        if (existingUser != null)
        {
            return AuthResult.Fail("Este e-mail já está cadastrado.");
        }

        var user = User.Create(email, name, _passwordHasher.Hash(request.Password), request.Role, _clock.UtcNow);

        await _userRepository.AddAsync(user, ct);
        LogUserRegistered(user.Id, email, ip ?? "unknown");

        await _eventDispatcher.DispatchAsync(
            new UserRegisteredEvent(user.Id, user.Email, user.Name, _clock.UtcNow), ct);

        return await IssueTokensAsync(user, ip, ct);
    }

    private async Task<AuthResult> IssueTokensAsync(User user, string? ip, CancellationToken ct, RefreshTokenResult? existingRefresh = null)
    {
        var access = _tokenGenerator.GenerateAccessToken(user);
        var refresh = existingRefresh ?? _tokenGenerator.GenerateRefreshToken(user.Id, ip);
        
        if (existingRefresh == null)
            await _refreshTokenRepository.AddAsync(refresh.Entity, ct);
        
        return AuthResult.Success(access.Token, refresh.RawToken, access.ExpiresAt, refresh.Entity.ExpiresAt, user);
    }

    #region Logs (source-generated)
    [LoggerMessage(Level = LogLevel.Information, Message = "Usuário registrado: {UserId} ({Email}) de {Ip}")]
    private partial void LogUserRegistered(string userId, string email, string ip);
    #endregion
}