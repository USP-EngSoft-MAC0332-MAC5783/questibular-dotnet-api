namespace Portfolio.Domain.Repositories;

public interface ITeacherClassRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<Entities.TeacherClass?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Entities.TeacherClass>> GetByTeacherIdAsync(string teacherId, CancellationToken ct);
    Task AddAsync(Entities.TeacherClass teacherClass, CancellationToken ct);
    Task<bool> DeleteAsync(string teacherId, string code, CancellationToken ct);
}