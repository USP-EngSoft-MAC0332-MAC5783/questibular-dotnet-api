namespace Portfolio.Api.Features.Exams;

// =============================================================================
// IExamFacade
// Contract from the application layer for the Exams feature.
// =============================================================================
public interface IExamFacade
{
    Task<GetExamsResponse> GetExamsAsync(GetExamsRequest request, CancellationToken ct = default);
    Task<StartExamResponse> StartExamAsync(StartExamRequest request, string? userId, CancellationToken ct = default);

    Task<FinishExamResponse> FinishExamAsync(FinishExamRequest request, string? userId, CancellationToken ct = default);

    Task<FinishExamResponse> GetReportAsync(string id, string? userId, CancellationToken ct = default);
    Task<StartExamResponse> GetNextQuestionsAsync(string attemptId, string? userId, string? anonymousToken, CancellationToken ct = default);
    Task<ExamResultsResponse> GetResultsAsync(
        string id, 
        string? userId, 
        string? anonymousToken, 
        int limit, 
        int offset, 
        CancellationToken ct = default);

    Task<StudentExamHistoryResponse> GetStudentHistoryAsync(
        string studentId, 
        string? requestingUserId, 
        int limit, 
        int offset, 
        CancellationToken ct = default);
}