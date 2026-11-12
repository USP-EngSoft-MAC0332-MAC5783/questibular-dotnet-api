namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// FinishExamQuestionDto
// Item da lista de questões enviada no POST /api/v1/exams/finish.
// Contém apenas o necessário para calcular o relatório (não repete alternativas).
// =============================================================================
public record FinishExamQuestionDto(
    string Title,
    int Index,
    string CorrectAlternative,
    string SelectedAlternative);