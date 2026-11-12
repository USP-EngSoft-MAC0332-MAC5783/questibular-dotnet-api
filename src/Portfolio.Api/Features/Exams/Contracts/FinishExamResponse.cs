namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// FinishExamResponse
// Resposta de POST /api/v1/exams/finish e GET /api/v1/exams/report/{id}.
// Agrupa o sumário da prova + o relatório da tentativa.
// =============================================================================
public record FinishExamResponse(
    ExamInfoDto Exam,
    ExamReportDto Report);