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
}