namespace Portfolio.Api.Features.ExamResults.Contracts;

/// <summary>Paginated metadata for the questions in an exam result.</summary>
public sealed record ExamResultMetadataResponse(
    int Limit,
    int Offset,
    int Total,
    bool HasMore);

/// <summary>One alternative belonging to an exam question.</summary>
public sealed record ExamAlternativeResponse(
    string Letter,
    string? Text,
    string? File,
    bool IsCorrect);

/// <summary>The alternative selected by the student, if any.</summary>
public sealed record ExamSelectedAlternativeResponse(string Letter, int Index);

/// <summary>A question and the student's answer as shown in an exam result.</summary>
public sealed record ExamResultQuestionResponse(
    string Title,
    int Index,
    string? Discipline,
    string? Language,
    int Year,
    string? Context,
    IReadOnlyList<string> Files,
    string? CorrectAlternative,
    string? AlternativesIntroduction,
    IReadOnlyList<ExamAlternativeResponse> Alternatives,
    ExamSelectedAlternativeResponse? SelectedAlternative);

/// <summary>Exam information, result timing, and a page of answered questions.</summary>
public sealed record ExamResultResponse(
    Guid Id,
    Guid Exam,
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
    ExamResultMetadataResponse Metadata,
    IReadOnlyList<ExamResultQuestionResponse> Questions);
