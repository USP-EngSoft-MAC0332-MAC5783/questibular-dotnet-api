public sealed record CreateTeacherClassRequest(
    [param: Required(ErrorMessage = "O código da turma é obrigatório.")]
    [param: MinLength(2, ErrorMessage = "O código da turma deve ter pelo menos 2 caracteres.")]
    [param: MaxLength(32, ErrorMessage = "O código da turma deve ter no máximo 32 caracteres.")]
    [param: RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "Use apenas letras, números, hífen ou underline (ex.: ESC-3A).")]
    string Code);

public sealed record TeacherClassResponse(
    string Id,
    string Code,
    DateTime CreatedAt);

public sealed record ClassStudentDto(
    string Id,
    string Name,
    string Email,
    string ClassCode,
    bool IsLinked);