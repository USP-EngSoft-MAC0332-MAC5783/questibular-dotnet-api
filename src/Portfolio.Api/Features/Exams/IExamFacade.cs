namespace Portfolio.Api.Features.Exams;

// =============================================================================
// IExamFacade
// Contract from the application layer for the Exams feature.
// =============================================================================
public interface IExamFacade
{
    Task<GetExamsResponse> GetExamsAsync(GetExamsRequest request, CancellationToken ct = default);
}