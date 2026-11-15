namespace Portfolio.DataAccess.Models;

internal sealed class ExamResultDbModel
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string StudentId { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public int TimeInMinutes { get; set; }
    public int QuestionCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public bool Humanities { get; set; }
    public bool Nature { get; set; }
    public bool Languages { get; set; }
    public bool Math { get; set; }
    public bool Anonymous { get; set; }
    public string? AnonymousToken { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishAt { get; set; }
    public string Answers { get; set; } = "[]";
}
