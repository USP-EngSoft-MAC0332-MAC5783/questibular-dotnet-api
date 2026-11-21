namespace Portfolio.Api.Features.Exams.Contracts;

public record ExamResultsResponse(
    string Id,
    string ExamId,
    int Year,
    DateTime StartedAt,
    DateTime? FinishedAt,
    ExamMetadataDto Metadata,
    IEnumerable<ExamResultQuestionDto> Questions);