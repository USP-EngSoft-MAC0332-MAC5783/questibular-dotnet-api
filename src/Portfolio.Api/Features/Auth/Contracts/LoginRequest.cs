namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record LoginRequest(
    [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [param: EmailAddress(ErrorMessage = "Por favor, insira um endereço de e-mail válido.")]
    [param: MaxLength(256, ErrorMessage = "O e-mail deve ter no máximo 256 caracteres.")] 
    string Email,

    [param: Required(ErrorMessage = "A senha é obrigatória.")]
    string Password,

    string? Code = null,
    string? PendingToken = null);