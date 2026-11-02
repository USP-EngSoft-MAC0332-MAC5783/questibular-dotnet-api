namespace Portfolio.Api.Features.Auth;

public interface IAuthFacade
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string? ip, CancellationToken ct);
}