namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// AlternativeDto
// Alternativa de uma questão — espelha o formato da API do ENEM.
// =============================================================================
public record AlternativeDto(
    string Letter,
    string Text,
    string? File,
    bool IsCorrect);