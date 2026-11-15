namespace Portfolio.Domain.Repositories;

using Portfolio.Domain.ReadModels;

public interface IExamResultRepository
{
    Task<IReadOnlyList<ExamResultReadModel>?> GetResultsForStudentAsync(
        string studentId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        CancellationToken cancellationToken);

    Task<ExamResultReadModel?> GetResultByIdAsync(
        Guid resultId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        CancellationToken cancellationToken);
}
