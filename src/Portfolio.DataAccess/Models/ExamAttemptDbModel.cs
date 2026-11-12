namespace Portfolio.DataAccess.Models;

internal sealed class ExamAttemptDbModel
{
    public string Id { get; set; } = default!;
    public string ExamId { get; set; } = default!;
    public string? UserId { get; set; }
    public bool IsAnonymous { get; set; }
    public string? AnonymousToken { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime PlannedFinishAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public int TimeInMinutes { get; set; }
    public int QuestionCount { get; set; }
    public string? SelectedLanguage { get; set; }
    
    public bool IncludeHumanities { get; set; }
    public bool IncludeNature { get; set; }
    public bool IncludeLanguages { get; set; }
    public bool IncludeMath { get; set; }

    public List<AttemptQuestionDbModel> ServedQuestions { get; set; } = [];
    public List<QuestionAnswerDbModel> Answers { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

internal sealed class AttemptQuestionDbModel
{
    public int Index { get; set; }
    public string Discipline { get; set; } = default!;
    public string? Language { get; set; }
}

internal sealed class QuestionAnswerDbModel
{
    public int Index { get; set; }
    public string CorrectAlternative { get; set; } = default!;
    public string SelectedAlternative { get; set; } = default!;
}