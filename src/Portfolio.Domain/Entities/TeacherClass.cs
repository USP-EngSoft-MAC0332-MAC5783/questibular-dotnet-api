namespace Portfolio.Domain.Entities;

public sealed class TeacherClass : EntityMetadata
{
    public string TeacherId { get; private set; } = default!;
    public string Code { get; private set; } = default!;

    private TeacherClass() { }

    public static TeacherClass Create(string teacherId, string code, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new ArgumentException("Identificador do professor inválido.");

        var normalizedCode = NormalizeCode(code)
            ?? throw new ArgumentException("O código da turma é obrigatório.");

        return new TeacherClass
        {
            Id = Guid.NewGuid().ToString("N"),
            TeacherId = teacherId.Trim(),
            Code = normalizedCode,
            CreatedAt = now
        };
    }

    public static TeacherClass Reconstitute(string id, string teacherId, string code, DateTime createdAt)
        => new()
        {
            Id = id,
            TeacherId = teacherId,
            Code = code,
            CreatedAt = createdAt
        };

    public static string? NormalizeCode(string? code)
        => string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
}