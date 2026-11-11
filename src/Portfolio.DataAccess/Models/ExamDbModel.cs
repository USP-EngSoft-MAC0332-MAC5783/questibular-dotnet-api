namespace Portfolio.DataAccess.Models;

internal sealed class ExamDbModel
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Year { get; set; } = default!;
    
    public int QuestionCountHumanities { get; set; }
    public int QuestionCountNature { get; set; }
    public int QuestionCountLanguages { get; set; }
    public int QuestionCountMath { get; set; }

    public bool IsSynced { get; set; }
    public int StartCount { get; set; }
    public int FinishCount { get; set; }
    public int TotalQuestionsFinished { get; set; }
    public DateTime? LastStartedAt { get; set; }
    public DateTime? LastFinishedAt { get; set; }

    public List<string> Languages { get; set; } = [];
    public List<string> Disciplines { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}