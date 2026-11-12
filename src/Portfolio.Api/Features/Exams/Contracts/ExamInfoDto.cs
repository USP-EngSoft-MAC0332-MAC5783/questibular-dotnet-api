namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// ExamInfoDto
// Dados resumidos da prova, embutidos no relatório de finish/report.
// =============================================================================
public record ExamInfoDto(
    string Id,
    string Year,
    int QuestionCountHumanities,
    int QuestionCountNature,
    int QuestionCountLanguages,
    int QuestionCountMath);