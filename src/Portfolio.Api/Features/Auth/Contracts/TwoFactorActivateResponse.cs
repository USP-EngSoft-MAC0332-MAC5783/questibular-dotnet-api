namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record TwoFactorActivateResponse(
    bool Enabled,
    IReadOnlyList<string> RecoveryCodes);