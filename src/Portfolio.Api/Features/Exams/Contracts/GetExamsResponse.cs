namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// GetExamsResponse
// GET /api/v1/exams: aggregate discipline count and the exam list
// in the range [start, end].
// =============================================================================
public record GetExamsResponse(
    int Start,
    int End,
    int TotalCountHumanities,
    int TotalCountNature,
    int TotalCountLanguages,
    int TotalCountMath,
    IEnumerable<ExamResponseDto> Exams);