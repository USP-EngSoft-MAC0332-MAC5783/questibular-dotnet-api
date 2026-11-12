namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// FinishExamRequest
// Payload de POST /api/v1/exams/finish.
// =============================================================================
public record FinishExamRequest(
    string Id,
    string Exam,
    bool Anonymous,
    string? AnonymousToken,
    IEnumerable<FinishExamQuestionDto> Questions);