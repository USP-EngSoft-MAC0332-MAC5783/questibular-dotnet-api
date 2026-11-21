namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// StartExamResponse
// Resposta de POST /api/v1/exams/start.
// =============================================================================
public record StartExamResponse(
    string Id,
    string Exam,
    string Year,
    int TimeInMinutes,
    int QuestionCount,
    string? Language,
    bool Humanities,
    bool Nature,
    bool Languages,
    bool Math,
    bool Anonymous,
    string? AnonymousToken,
    DateTime StartedAt,
    DateTime FinishAt,
    ExamMetadataDto Metadata,
    IEnumerable<QuestionDto> Questions);