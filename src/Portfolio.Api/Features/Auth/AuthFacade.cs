namespace Portfolio.Api.Features.Auth;

public sealed partial class AuthFacade : IAuthFacade
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const string TwoFactorIssuer = "Portfolio.Api";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITotpProvider _totp;
    private readonly ISecretProtector _protector;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly ILogger<AuthFacade> _logger;

    public AuthFacade(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        ITotpProvider totp,
        ISecretProtector protector,
        IDomainEventDispatcher eventDispatcher,
        IClock clock,
        ILogger<AuthFacade> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _totp = totp ?? throw new ArgumentNullException(nameof(totp));
        _protector = protector ?? throw new ArgumentNullException(nameof(protector));
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

    public async Task<AuthResult> LoginAsync(LoginRequest request, string? ip, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);

        if (user == null)
        {
            LogLoginFailed(request.Email, "User not found");
            return AuthResult.Fail("Credenciais inválidas.");
        }

        var now = _clock.UtcNow;

        if (user.IsLockedOutAt(now))
        {
            LogLoginFailed(user.Email, "Account locked");
            return AuthResult.Fail($"Conta bloqueada até {user.LockoutEnd:u}.");
        }

        var verification = _passwordHasher.Verify(user.PasswordHash, request.Password);

        if (verification == PasswordVerification.Failed)
        {
            user.RegisterFailedAccess(MaxFailedAttempts, LockoutDuration, now);
            await _userRepository.UpdateAsync(user, ct);

            if (user.IsLockedOutAt(now))
            {
                LogAccountLocked(user.Id, user.LockoutEnd!.Value);
                await _eventDispatcher.DispatchAsync(
                    new AccountLockedEvent(user.Id, user.LockoutEnd!.Value, ip, now), ct);
            }

            return AuthResult.Fail("Credenciais inválidas.");
        }

        user.RegisterSuccessfulLogin(now);
        await _userRepository.UpdateAsync(user, ct);

        LogLoginSuccess(user.Id, ip ?? "unknown");

        await _eventDispatcher.DispatchAsync(
            new UserLoggedInEvent(user.Id, ip, user.TwoFactorEnabled, now), ct);

        if (user.TwoFactorEnabled)
        {
            var pendingToken = _tokenGenerator.GeneratePendingTwoFactorToken(user.Id);
            return AuthResult.SuccessTwoFactor(pendingToken, user);
        }

        return await IssueTokensAsync(user, ip, ct);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return AuthResult.Fail("Refresh token não informado.");

        var hash = _tokenGenerator.HashRefreshToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByHashAsync(hash, ct);

        if (stored == null)
            return AuthResult.Fail("Refresh token inválido.");

        if (!stored.IsActiveAt(_clock.UtcNow))
        {
            if (stored.RevokedAt.HasValue)
            {
                LogTokenReused(stored.UserId);
                await _refreshTokenRepository.RevokeAllForUserAsync(stored.UserId, ip, _clock.UtcNow, ct);

                await _eventDispatcher.DispatchAsync(
                    new RefreshTokenReusedEvent(stored.UserId, stored.Id, ip, _clock.UtcNow), ct);
            }
            return AuthResult.Fail("Refresh token expirado ou revogado.");
        }

        var user = await _userRepository.GetByIdAsync(stored.UserId, ct);
        if (user == null) return AuthResult.Fail("Usuário não encontrado.");

        var newRefresh = _tokenGenerator.GenerateRefreshToken(user.Id, ip);
        var now = _clock.UtcNow;

        stored.Revoke(now, ip, newRefresh.Entity.TokenHash);
        await _refreshTokenRepository.UpdateAsync(stored, ct);
        await _refreshTokenRepository.AddAsync(newRefresh.Entity, ct);

        await _eventDispatcher.DispatchAsync(
            new RefreshTokenRotatedEvent(user.Id, stored.Id, newRefresh.Entity.Id, now), ct);

        return await IssueTokensAsync(user, ip, ct, newRefresh);
    }

    public async Task LogoutAsync(string refreshToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;

        var hash = _tokenGenerator.HashRefreshToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByHashAsync(hash, ct);
        
        if (stored == null || !stored.IsActiveAt(_clock.UtcNow)) return;

        stored.Revoke(_clock.UtcNow, ip);
        await _refreshTokenRepository.UpdateAsync(stored, ct);

        LogLogout(stored.UserId);
        await _eventDispatcher.DispatchAsync(
            new UserLoggedOutEvent(stored.UserId, ip, _clock.UtcNow), ct);
    }

    public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct) 
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (user.TwoFactorEnabled)
            throw new InvalidOperationException("2FA já está habilitado.");

        var (secret, otpAuthUri) = _totp.GenerateSecret(TwoFactorIssuer, user.Email);
        
        user.SetupTwoFactor(_protector.Protect(secret));
        await _userRepository.UpdateAsync(user, ct);

        LogTwoFactorSetup(userId);
        return new TwoFactorSetupResponse(secret, otpAuthUri);
    }

    public async Task<TwoFactorActivateResponse> ActivateTwoFactorAsync(string userId, string code, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct) 
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (user.TwoFactorEnabled) throw new InvalidOperationException("2FA já está habilitado.");
        if (string.IsNullOrEmpty(user.TwoFactorSecret)) throw new InvalidOperationException("Execute o setup antes.");

        var secret = _protector.Unprotect(user.TwoFactorSecret);
        if (!_totp.ValidateCode(secret, code)) throw new UnauthorizedAccessException("Código inválido.");

        var rawCodes = _totp.GenerateRecoveryCodes(10);
        var hashed = rawCodes.Select(_totp.HashRecoveryCode).ToList();

        user.EnableTwoFactor(hashed, _clock.UtcNow);
        await _userRepository.UpdateAsync(user, ct);

        await _eventDispatcher.DispatchAsync(new TwoFactorEnabledEvent(user.Id, _clock.UtcNow), ct);

        LogTwoFactorActivated(userId, rawCodes.Count);
        return new TwoFactorActivateResponse(true, rawCodes);
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string pendingToken, string code, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pendingToken) || string.IsNullOrWhiteSpace(code))
            return AuthResult.Fail("Token ou código não informado.");

        if (!_tokenGenerator.TryGetUserIdFromPendingToken(pendingToken, out var userId))
            return AuthResult.Fail("Token 2FA inválido ou expirado.");

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) return AuthResult.Fail("Usuário não encontrado.");

        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            return AuthResult.Fail("2FA não está habilitado.");

        var secret = _protector.Unprotect(user.TwoFactorSecret);

        if (_totp.ValidateCode(secret, code))
        {
            LogTwoFactorVerified(userId);
            return await IssueTokensAsync(user, ip, ct);
        }

        if (user.UseRecoveryCode(_totp.HashRecoveryCode(code))) // Supondo método rico no Domain
        {
            await _userRepository.UpdateAsync(user, ct);
            LogRecoveryCodeUsed(userId, user.TwoFactorRecoveryCodes.Count);
            return await IssueTokensAsync(user, ip, ct);
        }

        return AuthResult.Fail("Código 2FA inválido.");
    }

    public async Task DisableTwoFactorAsync(string userId, string code, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct) 
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (!user.TwoFactorEnabled) throw new InvalidOperationException("2FA não está habilitado.");

        var secret = _protector.Unprotect(user.TwoFactorSecret!);
        var totpValid = _totp.ValidateCode(secret, code);
        
        if (!totpValid && !user.UseRecoveryCode(_totp.HashRecoveryCode(code)))
            throw new UnauthorizedAccessException("Código inválido.");

        user.DisableTwoFactor();
        await _userRepository.UpdateAsync(user, ct);

        await _eventDispatcher.DispatchAsync(new TwoFactorDisabledEvent(user.Id, _clock.UtcNow), ct);
        LogTwoFactorDisabled(userId);
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Login efetuado: {UserId} de {Ip}")]
    private partial void LogLoginSuccess(string userId, string ip);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Falha no login para {Email}. Motivo: {Reason}")]
    private partial void LogLoginFailed(string email, string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Conta bloqueada: {UserId} até {LockoutEnd:O}")]
    private partial void LogAccountLocked(string userId, DateTime lockoutEnd);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh token reutilizado. Revogando família: {UserId}")]
    private partial void LogTokenReused(string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Logout efetuado para o usuário {UserId}")]
    private partial void LogLogout(string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Setup de 2FA iniciado para {UserId}")]
    private partial void LogTwoFactorSetup(string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "2FA ativado para {UserId}. {Count} recovery codes gerados.")]
    private partial void LogTwoFactorActivated(string userId, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "2FA verificado via TOTP: {UserId}")]
    private partial void LogTwoFactorVerified(string userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Usuário {UserId} usou recovery code. Restantes: {Remaining}")]
    private partial void LogRecoveryCodeUsed(string userId, int remaining);

    [LoggerMessage(Level = LogLevel.Information, Message = "2FA desativado para {UserId}")]
    private partial void LogTwoFactorDisabled(string userId);
    #endregion
}