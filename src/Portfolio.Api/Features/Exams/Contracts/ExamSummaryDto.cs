namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// ExamSummaryDto
// Percentuais de acerto por disciplina + percentual total.
// Os valores são strings formatadas com uma casa decimal (ex.: "75.0").
// =============================================================================
public record ExamSummaryDto(
    string TotalPercentage,
    string Humanities,
    string Nature,
    string Languages,
    string Math);