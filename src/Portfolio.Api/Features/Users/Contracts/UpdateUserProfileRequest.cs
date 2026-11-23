namespace Portfolio.Api.Features.Users.Contracts;

public sealed record UpdateUserProfileRequest(
    [param: Required(ErrorMessage = "O nome é obrigatório.")]
    [param: MinLength(3, ErrorMessage = "O nome deve ter pelo menos 3 caracteres.")]
    [param: MaxLength(64, ErrorMessage = "O nome deve ter no máximo 64 caracteres.")]
    string Name,

    [param: MaxLength(32, ErrorMessage = "O código da turma deve ter no máximo 32 caracteres.")]
    string? ClassCode = null);