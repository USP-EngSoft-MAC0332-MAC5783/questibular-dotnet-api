namespace Portfolio.Domain.Entities;

// =============================================================================
// Represents an ENEM exam obtained from the external API.
//
// Regras de rastreio:
//  • IsSynced ................ indicates if the exam has been fully persisted
//                              (true) or is a "stub" waiting for synchronization (false).
//                              When false, an external process fetches the data from
//                              the ENEM API and completes the record.
//  • StartCount .............. how many times the exam has been started.
//  • FinishCount ............. how many times the exam has been finished.
//  • TotalQuestionsFinished .. sum of questions finished across all attempts.
//  • LastStartedAt/FinishedAt  UTC timestamps of the last events.
// =============================================================================
public sealed class Exam : EntityMetadata
{
    public string Title { get; private set; } = default!;
    public string Year { get; private set; }

    public int QuestionCountHumanities { get; private set; }
    public int QuestionCountNature     { get; private set; }
    public int QuestionCountLanguages  { get; private set; }
    public int QuestionCountMath       { get; private set; }

    /// <summary>
    /// Indicates if the exam has been fully persisted
    /// (true) or is a "stub" waiting for synchronization (false).
    /// When false, an external process fetches the data from
    /// the ENEM API and completes the record.
    /// </summary>
    public bool IsSynced { get; private set; }

    public int StartCount              { get; private set; }
    public int FinishCount             { get; private set; }
    public int TotalQuestionsFinished  { get; private set; }
    public DateTime? LastStartedAt     { get; private set; }
    public DateTime? LastFinishedAt    { get; private set; }

    private readonly List<string> _languages = [];
    public IReadOnlyCollection<string> Languages => _languages.AsReadOnly();

    private readonly List<string> _disciplines = [];
    public IReadOnlyCollection<string> Disciplines => _disciplines.AsReadOnly();

    private Exam() { }

    /// <summary>
    /// Creates an exam that is already synchronized with the API (IsSynced = true).
    /// Used by the synchronization process when persisting an exam from the ENEM API.
    /// </summary>
    public static Exam Create(
        string title,
        string year,
        int humanities,
        int nature,
        int languages,
        int math,
        IEnumerable<string> availableDisciplines,
        IEnumerable<string> availableLanguages,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Título inválido.");
        if (string.IsNullOrWhiteSpace(year) || year.Length != 4 || !int.TryParse(year, out _)) 
            throw new ArgumentException("Ano inválido.");

        var exam = new Exam
        {
            Id                       = Guid.NewGuid().ToString("N"),
            Title                    = title,
            Year                     = year,
            QuestionCountHumanities  = humanities,
            QuestionCountNature      = nature,
            QuestionCountLanguages   = languages,
            QuestionCountMath        = math,
            IsSynced                 = true,
            CreatedAt                = now
        };

        exam._disciplines.AddRange(availableDisciplines);
        exam._languages.AddRange(availableLanguages);

        return exam;
    }

    /// <summary>
    /// Creates a "stub" exam (not synchronized). Only the year is known.
    /// A background process will detect IsSynced = false, fetch from the ENEM API
    /// and complete the data via <see cref="ApplySyncData"/>.
    /// </summary>
    public static Exam CreateStub(string year, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(year) || year.Length != 4 || !int.TryParse(year, out _))
            throw new ArgumentException("Ano inválido.");

        return new Exam
        {
            Id        = Guid.NewGuid().ToString("N"),
            Title     = $"ENEM {year}",
            Year      = year,
            IsSynced  = false,
            CreatedAt = now
        };
    }

    /// <summary>
    /// Reconstitutes the entity from persistence (used by the Infrastructure).
    /// </summary>
    public static Exam Reconstitute(
        string id,
        string title,
        string year,
        int humanities,
        int nature,
        int languages,
        int math,
        IEnumerable<string> availableDisciplines,
        IEnumerable<string> availableLanguages,
        bool isSynced,
        int startCount,
        int finishCount,
        int totalQuestionsFinished,
        DateTime? lastStartedAt,
        DateTime? lastFinishedAt,
        DateTime createdAt)
    {
        var exam = new Exam
        {
            Id                      = id,
            Title                   = title,
            Year                    = year,
            QuestionCountHumanities = humanities,
            QuestionCountNature     = nature,
            QuestionCountLanguages  = languages,
            QuestionCountMath       = math,
            IsSynced                = isSynced,
            StartCount              = startCount,
            FinishCount             = finishCount,
            TotalQuestionsFinished  = totalQuestionsFinished,
            LastStartedAt           = lastStartedAt,
            LastFinishedAt          = lastFinishedAt,
            CreatedAt               = createdAt
        };

        exam._disciplines.AddRange(availableDisciplines);
        exam._languages.AddRange(availableLanguages);
        return exam;
    }

    /// <summary>
    /// Registers that the exam has been started by someone.
    /// </summary>
    public void RegisterStart(DateTime now)
    {
        StartCount++;
        LastStartedAt = now;
        UpdatedAt     = now;
    }

    /// <summary>
    /// Saves an exam attempt.
    /// </summary>
    /// <param name="questionsFinished">Quantidade de questões respondidas nesta tentativa.</param>
    public void RegisterFinish(int questionsFinished, DateTime now)
    {
        if (questionsFinished < 0) throw new ArgumentOutOfRangeException(nameof(questionsFinished));

        FinishCount++;
        TotalQuestionsFinished += questionsFinished;
        LastFinishedAt = now;
        UpdatedAt      = now;
    }

    /// <summary>
    /// Applies the synchronization data received from the external API.
    /// </summary>
    public void ApplySyncData(
        string title,
        int humanities,
        int nature,
        int languages,
        int math,
        IEnumerable<string> disciplines,
        IEnumerable<string> languagesList,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Título inválido.");

        Title                   = title;
        QuestionCountHumanities = humanities;
        QuestionCountNature     = nature;
        QuestionCountLanguages  = languages;
        QuestionCountMath       = math;

        _disciplines.Clear();
        _disciplines.AddRange(disciplines);

        _languages.Clear();
        _languages.AddRange(languagesList);

        IsSynced  = true;
        UpdatedAt = now;
    }
}