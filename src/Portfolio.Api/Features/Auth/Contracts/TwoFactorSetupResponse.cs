namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record TwoFactorSetupResponse(
    string Secret,
    string OtpAuthUri);