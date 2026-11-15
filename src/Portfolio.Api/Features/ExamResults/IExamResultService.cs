namespace Portfolio.Api.Features.ExamResults;

using Portfolio.Api.Features.ExamResults.Contracts;

public interface IExamResultService
{
    Task<IReadOnlyList<ExamResultResponse>?> GetResultsForStudentAsync(
        Guid studentId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        int limit,
        int offset,
        CancellationToken cancellationToken);

    Task<ExamResultResponse?> GetResultByIdAsync(
        Guid resultId,
        string requestingUserId,
        bool isStudent,
        bool isTeacher,
        int limit,
        int offset,
        CancellationToken cancellationToken);
}
