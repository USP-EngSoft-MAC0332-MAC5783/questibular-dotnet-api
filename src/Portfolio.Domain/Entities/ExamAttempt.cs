namespace Portfolio.Domain.Entities;

// =============================================================================
// ExamAttempt + AttemptQuestion + QuestionAnswer
// It registers an exam attempt initiated by a user or anonymous.
//
// It saves the following information:
//  • ServedQuestions ... question's metadata served on start (allow
//                        the calculation of correct answers by subject).
//  • Answers ........... responses sent by the client on finish.
//  • ExpiresAt ......... TTL for automatic cleanup of anonymous attempts
//                        (PlannedFinishAt + 1h according to acceptance criteria).
// =============================================================================
public sealed class ExamAttempt : EntityMetadata
{
    public string ExamId  { get; private set; } = default!;
    public string? UserId { get; private set; }
    public bool IsAnonymous { get; private set; }
    public string? AnonymousToken { get; private set; }

    public DateTime StartedAt        { get; private set; }
    public DateTime PlannedFinishAt  { get; private set; }
    public DateTime? FinishedAt      { get; private set; }

    /// <summary>TTL (just for anonymous attempts).</summary>
    public DateTime? ExpiresAt       { get; private set; }

    public int TimeInMinutes    { get; private set; }
    public int QuestionCount    { get; private set; }
    public string? SelectedLanguage { get; private set; }

    public bool IncludeHumanities { get; private set; }
    public bool IncludeNature     { get; private set; }
    public bool IncludeLanguages  { get; private set; }
    public bool IncludeMath       { get; private set; }

    private readonly List<AttemptQuestion> _servedQuestions = [];
    public IReadOnlyCollection<AttemptQuestion> ServedQuestions => _servedQuestions.AsReadOnly();

    private readonly List<QuestionAnswer> _answers = [];
    public IReadOnlyCollection<QuestionAnswer> Answers => _answers.AsReadOnly();

    private ExamAttempt() { }

    public static ExamAttempt Create(
        string examId,
        string? userId,
        bool isAnonymous,
        string? anonymousToken,
        int timeInMinutes,
        int questionCount,
        string? selectedLanguage,
        bool humanities,
        bool nature,
        bool languages,
        bool math,
        DateTime now)
    {
        if (!isAnonymous && string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("Usuário autenticado é obrigatório para simulados não anônimos.");

        if (isAnonymous && string.IsNullOrWhiteSpace(anonymousToken))
            anonymousToken = Guid.NewGuid().ToString("N");

        var plannedFinish = now.AddMinutes(timeInMinutes);

        return new ExamAttempt
        {
            Id                = Guid.NewGuid().ToString("N"),
            ExamId            = examId,
            UserId            = userId,
            IsAnonymous       = isAnonymous,
            AnonymousToken    = anonymousToken,
            TimeInMinutes     = timeInMinutes,
            QuestionCount     = questionCount,
            SelectedLanguage  = selectedLanguage,
            IncludeHumanities = humanities,
            IncludeNature     = nature,
            IncludeLanguages  = languages,
            IncludeMath       = math,
            StartedAt         = now,
            PlannedFinishAt   = plannedFinish,
            ExpiresAt         = isAnonymous ? plannedFinish.AddHours(1) : null,
            CreatedAt         = now
        };
    }

    public static ExamAttempt Reconstitute(
        string id,
        string examId,
        string? userId,
        bool isAnonymous,
        string? anonymousToken,
        DateTime startedAt,
        DateTime plannedFinishAt,
        DateTime? finishedAt,
        DateTime? expiresAt,
        int timeInMinutes,
        int questionCount,
        string? selectedLanguage,
        bool humanities,
        bool nature,
        bool languages,
        bool math,
        IEnumerable<AttemptQuestion> servedQuestions,
        IEnumerable<QuestionAnswer> answers,
        DateTime createdAt)
    {
        var attempt = new ExamAttempt
        {
            Id                = id,
            ExamId            = examId,
            UserId            = userId,
            IsAnonymous       = isAnonymous,
            AnonymousToken    = anonymousToken,
            StartedAt         = startedAt,
            PlannedFinishAt   = plannedFinishAt,
            FinishedAt        = finishedAt,
            ExpiresAt         = expiresAt,
            TimeInMinutes     = timeInMinutes,
            QuestionCount     = questionCount,
            SelectedLanguage  = selectedLanguage,
            IncludeHumanities = humanities,
            IncludeNature     = nature,
            IncludeLanguages  = languages,
            IncludeMath       = math,
            CreatedAt         = createdAt
        };

        attempt._servedQuestions.AddRange(servedQuestions);
        attempt._answers.AddRange(answers);
        return attempt;
    }

    public void RegisterServedQuestions(IEnumerable<AttemptQuestion> questions, DateTime now)
    {
        _servedQuestions.Clear();
        _servedQuestions.AddRange(questions);
        UpdatedAt = now;
    }

    public void RegisterAnswers(IEnumerable<QuestionAnswer> answers, DateTime now)
    {
        _answers.Clear();
        _answers.AddRange(answers);
        UpdatedAt = now;
    }

    public void Finish(DateTime now)
    {
        if (FinishedAt.HasValue)
            throw new InvalidOperationException("Simulado já finalizado.");

        FinishedAt = now;
        UpdatedAt  = now;
    }

    // Adiciona questões sem apagar as anteriores (ideal para paginação)
    public void AddServedQuestions(IEnumerable<AttemptQuestion> questions, DateTime now)
    {
        _servedQuestions.AddRange(questions);
        UpdatedAt = now;
    }
}