namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// ExamResponseDto
// DTO represents the output for an exam within the list (US05).
// The "languages" field is serialized in snake-case from the ENEM API (English, Spanish).
// =============================================================================
public record ExamResponseDto(
    string Id,
    string Title,
    string Year,
    int QuestionCountHumanities,
    int QuestionCountNature,
    int QuestionCountLanguages,
    int QuestionCountMath,
    IEnumerable<string> Languages);