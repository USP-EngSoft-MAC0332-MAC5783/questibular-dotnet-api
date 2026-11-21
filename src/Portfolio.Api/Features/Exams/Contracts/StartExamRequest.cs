namespace Portfolio.Api.Features.Exams.Contracts;

// =============================================================================
// StartExamRequest
// Payload de POST /api/v1/exams/start.
// "id" preenchido = continuação (paginação); vazio = novo simulado.
// "anonymous" = true dispensa autenticação e usa anonymousToken como rastreio.
// =============================================================================
public record StartExamRequest(
    string? Id,
    string Year,
    int TimeInMinutes,
    int QuestionCount,
    string? Language,
    bool Humanities,
    bool Nature,
    bool Languages,
    bool Math,
    bool Anonymous,
    string? AnonymousToken);