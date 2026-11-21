namespace Portfolio.Api.Features.Exams.Contracts;

public record StudentExamHistoryResponse(
    IEnumerable<StudentExamHistoryItem> Exams);