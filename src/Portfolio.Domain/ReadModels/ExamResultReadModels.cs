namespace Portfolio.Domain.ReadModels;

public sealed record ExamResultReadModel(
    Guid Id,
    Guid ExamId,
    string Title,
    int Year,
    int TimeInMinutes,
    int QuestionCount,
    string Language,
    bool Humanities,
    bool Nature,
    bool Languages,
    bool Math,
    bool Anonymous,
    string? AnonymousToken,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishAt,
    IReadOnlyList<ExamAnswerReadModel> Answers);

public sealed record ExamAnswerReadModel(int QuestionIndex, string? SelectedAlternativeLetter);
