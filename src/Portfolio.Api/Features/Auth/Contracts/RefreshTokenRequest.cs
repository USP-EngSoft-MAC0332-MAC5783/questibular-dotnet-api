namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record RefreshTokenRequest(
    [param: Required] string RefreshToken);