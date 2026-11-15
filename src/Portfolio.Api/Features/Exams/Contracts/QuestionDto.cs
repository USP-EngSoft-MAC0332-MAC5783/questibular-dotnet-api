namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// QuestionDto
// Questão retornada em POST /api/v1/exams/start (e na paginação via "id").
// =============================================================================
public record QuestionDto(
    string Title,
    int Index,
    string? Discipline,
    string? Language,
    int Year,
    string Context,
    IEnumerable<string> Files,
    string CorrectAlternative,
    string AlternativesIntroduction,
    IEnumerable<AlternativeDto> Alternatives);