namespace Portfolio.Api.Features.ExamResults.Contracts;

public sealed record LinkedStudentDto(
    string Id,
    string Name,
    string Email,
    string? LastExamAttemptId,
    string? LastExamAccuracy,
    DateTime? LastExamFinishedAt);