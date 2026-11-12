namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// ExamMetadataDto
// Metadados de paginação para a lista de questões servida no start.
// Espelha o objeto "metadata" da API do ENEM.
// =============================================================================
public record ExamMetadataDto(int Limit, int Offset, int Total, bool HasMore);