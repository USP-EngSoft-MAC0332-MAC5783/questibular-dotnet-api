namespace Portfolio.Api.Features.Users.Contracts;

public sealed record StudentScoreResponse(
    int ExamAttemptCount,
    int NumberOfQuestionsSolved,
    DateTime? LastExamAttempt,
    string Average,
    string BestScore,
    string? BestScoreDiscipline,
    PerformanceDisciplinesDto PerformanceDisciplines,
    IReadOnlyList<ScoreEvolutionItemDto> Evolution);