namespace Portfolio.Api.Features.Users.Contracts;

public sealed record ScoreEvolutionItemDto(
    string Id,
    string ExamId,
    string General,
    string Humanities,
    string Nature,
    string Languages,
    string Math,
    DateTime Attempt);