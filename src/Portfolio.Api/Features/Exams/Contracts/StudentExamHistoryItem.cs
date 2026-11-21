namespace Portfolio.Api.Features.Exams.Contracts;

public record StudentExamHistoryItem(
    ExamInfoDto Exam,
    ExamReportDto Report);