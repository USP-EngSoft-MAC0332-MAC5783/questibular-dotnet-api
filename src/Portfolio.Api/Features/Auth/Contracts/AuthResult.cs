namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record AuthResult(
    bool Succeeded,
    string? AccessToken,
    string? RefreshToken,
    DateTime? AccessTokenExpiresAt,
    DateTime? RefreshTokenExpiresAt,
    string? UserId,
    string? Email,
    string? Name,
    List<string>? Roles,
    bool RequiresTwoFactor,
    string? PendingTwoFactorToken,
    string? ErrorMessage)
{
    public static AuthResult Fail(string message) 
        => new(false, null, null, null, null, null, null, null, null, false, null, message);

    public static AuthResult Success(string accessToken, string refreshToken, DateTime accessExp, DateTime refreshExp, Domain.Entities.User user) 
        => new(true, accessToken, refreshToken, accessExp, refreshExp, user.Id, user.Email, user.Name, user.Roles.ToList(), false, null, null);

    public static AuthResult SuccessTwoFactor(string pendingToken, Domain.Entities.User user) 
        => new(true, null, null, null, null, user.Id, user.Email, user.Name, user.Roles.ToList(), true, pendingToken, null);
}