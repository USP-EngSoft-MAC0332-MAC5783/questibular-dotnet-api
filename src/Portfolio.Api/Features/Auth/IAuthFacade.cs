namespace Portfolio.Api.Features.Auth;

public interface IAuthFacade
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string? ip, CancellationToken ct);
    Task<AuthResult> LoginAsync(LoginRequest request, string? ip, CancellationToken ct);
    Task<AuthResult> RefreshAsync(string refreshToken, string? ip, CancellationToken ct);
    Task LogoutAsync(string refreshToken, string? ip, CancellationToken ct);
    
    Task<TwoFactorSetupResponse> SetupTwoFactorAsync(string userId, CancellationToken ct);
    Task<TwoFactorActivateResponse> ActivateTwoFactorAsync(string userId, string code, CancellationToken ct);
    Task<AuthResult> VerifyTwoFactorAsync(string pendingToken, string code, string? ip, CancellationToken ct);
    Task DisableTwoFactorAsync(string userId, string code, CancellationToken ct);
}