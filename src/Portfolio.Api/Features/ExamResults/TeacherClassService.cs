namespace Portfolio.Api.Features.ExamResults;

public interface ITeacherClassService
{
    Task<TeacherClassResponse> CreateClassAsync(string teacherId, string code, CancellationToken ct);
    Task<IReadOnlyList<TeacherClassResponse>> GetMyClassesAsync(string teacherId, CancellationToken ct);
    Task<bool> DeleteClassAsync(string teacherId, string code, CancellationToken ct);
    Task<IReadOnlyList<ClassStudentDto>> GetClassStudentsAsync(string teacherId, string code, CancellationToken ct);
}

public sealed class TeacherClassService(
    ITeacherClassRepository classes,
    IUserRepository users,
    ITeacherStudentRepository links,
    IClock clock) : ITeacherClassService
{
    public async Task<TeacherClassResponse> CreateClassAsync(string teacherId, string code, CancellationToken ct)
    {
        var normalizedCode = TeacherClass.NormalizeCode(code)
            ?? throw new ArgumentException("O código da turma é obrigatório.");

        if (await classes.ExistsByCodeAsync(normalizedCode, ct))
            throw new InvalidOperationException($"Já existe uma turma cadastrada com o código '{normalizedCode}'.");

        var entity = TeacherClass.Create(teacherId, normalizedCode, clock.UtcNow);
        await classes.AddAsync(entity, ct);

        return new TeacherClassResponse(entity.Id, entity.Code, entity.CreatedAt);
    }

    public async Task<IReadOnlyList<TeacherClassResponse>> GetMyClassesAsync(string teacherId, CancellationToken ct)
    {
        var items = await classes.GetByTeacherIdAsync(teacherId, ct);
        return items.Select(c => new TeacherClassResponse(c.Id, c.Code, c.CreatedAt)).ToList();
    }

    public Task<bool> DeleteClassAsync(string teacherId, string code, CancellationToken ct)
        => classes.DeleteAsync(teacherId, code, ct);

    public async Task<IReadOnlyList<ClassStudentDto>> GetClassStudentsAsync(string teacherId, string code, CancellationToken ct)
    {
        var normalizedCode = TeacherClass.NormalizeCode(code)
            ?? throw new ArgumentException("O código da turma é obrigatório.");

        var teacherClass = await classes.GetByCodeAsync(normalizedCode, ct)
            ?? throw new KeyNotFoundException("Turma não encontrada.");

        if (!string.Equals(teacherClass.TeacherId, teacherId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Você não tem permissão para consultar os alunos desta turma.");

        var studentsInClass = await users.GetStudentsByClassCodeAsync(normalizedCode, ct);
        if (studentsInClass.Count == 0)
            return [];

        var linkedStudents = await links.GetLinkedStudentsAsync(teacherId, ct);
        var linkedIds = linkedStudents.Select(s => s.Id).ToHashSet(StringComparer.Ordinal);

        return studentsInClass
            .Select(s => new ClassStudentDto(
                Id: s.Id,
                Name: s.Name,
                Email: s.Email,
                ClassCode: s.ClassCode!,
                IsLinked: linkedIds.Contains(s.Id)))
            .ToList();
    }
}