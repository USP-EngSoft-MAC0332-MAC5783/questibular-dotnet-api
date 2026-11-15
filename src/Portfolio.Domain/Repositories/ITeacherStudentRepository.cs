namespace Portfolio.Domain.Repositories;

public interface ITeacherStudentRepository
{
    Task<bool> CreateLinkAsync(
        string teacherId,
        string studentId,
        CancellationToken cancellationToken);
}
