namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record TwoFactorCodeRequest(
    [param: Required(ErrorMessage = "O código é obrigatório.")]
    [param: StringLength(20, MinimumLength = 6, ErrorMessage = "O código deve ter entre 6 e 20 caracteres.")]
    string Code);