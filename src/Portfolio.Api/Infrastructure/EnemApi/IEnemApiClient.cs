namespace Portfolio.Api.Infrastructure.EnemApi;

// =============================================================================
// IEnemApiClient + DTOs
// Client to consume the external ENEM API.
//
// DTOs reflects the format of the response from the endpoints:
// =============================================================================
public interface IEnemApiClient
{
    Task<IReadOnlyList<EnemApiExamSummary>> GetAllExamsAsync(CancellationToken ct = default);
    Task<EnemApiExam?> GetExamByYearAsync(int year, CancellationToken ct = default);

    Task<EnemApiQuestionsPage> GetQuestionsAsync(
        int year,
        string? language,
        int limit,
        int offset,
        CancellationToken ct = default);
}

// ================== DTOs ==================
public sealed class EnemApiExamSummary
{
    public string Title { get; set; } = default!;
    public int Year { get; set; }
    public List<EnemApiLabelValue> Disciplines { get; set; } = [];
    public List<EnemApiLabelValue> Languages   { get; set; } = [];
}

public sealed class EnemApiExam
{
    public string Title { get; set; } = default!;
    public int Year { get; set; }
    public List<EnemApiLabelValue> Disciplines { get; set; } = [];
    public List<EnemApiLabelValue> Languages   { get; set; } = [];
    public List<EnemApiExamQuestionRef> Questions { get; set; } = [];
}

public sealed class EnemApiLabelValue
{
    public string Label { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public sealed class EnemApiExamQuestionRef
{
    public string Title { get; set; } = default!;
    public int Index { get; set; }
    public string? Discipline { get; set; }
    public string? Language { get; set; }
}

public sealed class EnemApiQuestionsPage
{
    public EnemApiPagination Metadata { get; set; } = new();
    public List<EnemApiQuestion> Questions { get; set; } = [];
}

public sealed class EnemApiPagination
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int Total { get; set; }
    public bool HasMore { get; set; }
}

public sealed class EnemApiQuestion
{
    public string Title { get; set; } = default!;
    public int Index { get; set; }
    public string? Discipline { get; set; }
    public string? Language { get; set; }
    public int Year { get; set; }
    public string Context { get; set; } = default!;
    public List<string> Files { get; set; } = [];
    public string CorrectAlternative { get; set; } = default!;
    public string AlternativesIntroduction { get; set; } = default!;
    public List<EnemApiAlternative> Alternatives { get; set; } = [];
}

public sealed class EnemApiAlternative
{
    public string Letter { get; set; } = default!;
    public string Text { get; set; } = default!;
    public string? File { get; set; }
    public bool IsCorrect { get; set; }
}