namespace Portfolio.Domain.Repositories;

public interface ITeacherStudentRepository
{
    Task<bool> CreateLinkAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken);

    Task<bool> HasLinkAsync(string teacherId, string studentId, CancellationToken ct);
}
