namespace Portfolio.Api.Features.Auth.Contracts;

public sealed record RegisterRequest(
    [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [param: EmailAddress(ErrorMessage = "Por favor, insira um endereço de e-mail válido.")]
    [param: MaxLength(256, ErrorMessage = "O e-mail deve ter no máximo 256 caracteres.")] 
    string Email,

    [param: Required(ErrorMessage = "O nome é obrigatório.")]
    [param: MinLength(3, ErrorMessage = "O nome deve ter pelo menos 3 caracteres.")]
    [param: MaxLength(64, ErrorMessage = "O nome deve ter no máximo 64 caracteres.")] 
    string Name,

    [property: JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
    [param: Required(ErrorMessage = "O perfil (Role) é obrigatório.")] 
    UserRole Role,

    [param: Required(ErrorMessage = "A senha é obrigatória.")]
    [param: MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    [param: MaxLength(128, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")] 
    string Password);
