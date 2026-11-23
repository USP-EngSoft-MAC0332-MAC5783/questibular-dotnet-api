namespace Portfolio.Api.Features.Exams.Contracts;

public record StudentExamHistoryItem(
    string AttemptId,
    ExamInfoDto Exam,
    ExamReportDto Report);