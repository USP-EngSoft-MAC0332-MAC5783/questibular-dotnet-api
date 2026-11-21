namespace Portfolio.Api.Features.Exams.Contracts;

public record ExamResultQuestionDto(
    string Title,
    int Index,
    string? Discipline,
    string? Language,
    int Year,
    string Context,
    IEnumerable<string> Files,
    string CorrectAlternative,
    string? SelectedAlternative, // Pode ser nulo se o aluno não respondeu
    bool IsCorrect,
    string AlternativesIntroduction,
    IEnumerable<AlternativeDto> Alternatives);