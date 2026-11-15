namespace Portfolio.Api.Features.ExamResults;

public interface ITeacherStudentService
{
    Task<bool> LinkStudentAsync(
        string teacherId,
        Guid studentId,
        CancellationToken cancellationToken);
}

public sealed class TeacherStudentService(ITeacherStudentRepository links) : ITeacherStudentService
{
    public Task<bool> LinkStudentAsync(
        string teacherId,
        Guid studentId,
        CancellationToken cancellationToken)
        => links.CreateLinkAsync(teacherId, studentId.ToString("N"), cancellationToken);
}
