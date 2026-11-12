namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// ExamReportDto
// Relatório completo de um simulado finalizado.
// =============================================================================
public record ExamReportDto(
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
    DateTime PlannedFinishAt,
    DateTime FinishAt,
    Dictionary<string, int> CorrectAnswers,
    ExamSummaryDto Summary);