namespace Portfolio.Api.Features.Exams;

// =============================================================================
// ExamFacade
// Orchestrate the application logic of the Exams feature.
//
// IMPORTANTE:
//  • GetExamsAsync create "stubs" (IsSynced = false) for exams not in db.
//    Exam.ApplySyncData(...) persists the complete exam data.
// =============================================================================

public sealed class ExamFacade : IExamFacade
{
    private readonly IExamRepository _examRepository;
    private readonly IExamAttemptRepository _attemptRepository;
    private readonly IEnemApiClient _enemApi;
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;
    private readonly IExamSyncService _examSyncService;

    public ExamFacade(
        IExamRepository examRepository,
        IExamAttemptRepository attemptRepository,
        IEnemApiClient enemApi,
        IUserRepository userRepository,
        IClock clock,
        IExamSyncService examSyncService)
    {
        _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
        _attemptRepository = attemptRepository ?? throw new ArgumentNullException(nameof(attemptRepository));
        _enemApi = enemApi ?? throw new ArgumentNullException(nameof(enemApi));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _examSyncService = examSyncService ?? throw new ArgumentNullException(nameof(examSyncService));
    }

    public async Task<GetExamsResponse> GetExamsAsync(GetExamsRequest request, CancellationToken ct = default)
    {
        if (request.Start > request.End)
            throw new ArgumentException("Parametro start não pode ser maior que end.");
        if (request.Start < 2000)
            throw new ArgumentException("Ano inicial inválido. Deve ser maior ou igual a 2000.");

        var existing = await _examRepository.GetByYearRangeAsync(request.Start, request.End, ct);
        var existingYears = existing.Select(e => e.Year).ToHashSet();

        var stubs = new List<Exam>();
        for (var year = request.Start; year <= request.End; year++)
        {
            var yearStr = year.ToString();
        
            if (!existingYears.Contains(yearStr))
            {
                stubs.Add(Exam.CreateStub(yearStr, _clock.UtcNow));
            }
        }

        if (stubs.Count > 0)
        {
            await _examRepository.AddRangeAsync(stubs, ct);
            existing.AddRange(stubs);
        }

        await _examSyncService.SyncIfNeededAsync(existing, ct);

        var ordered = existing.OrderBy(e => e.Year).ToList();
        
        int totalH = 0, totalN = 0, totalL = 0, totalM = 0;
        foreach (var e in ordered)
        {
            totalH += e.QuestionCountHumanities;
            totalN += e.QuestionCountNature;
            totalL += e.QuestionCountLanguages;
            totalM += e.QuestionCountMath;
        }

        return new GetExamsResponse(
            Start: request.Start,
            End: request.End,
            TotalCountHumanities: totalH,
            TotalCountNature: totalN,
            TotalCountLanguages: totalL,
            TotalCountMath: totalM,
            Exams: ordered.Select(MapExamToDto).ToList());
    }

    public async Task<StartExamResponse> StartExamAsync(StartExamRequest request, string? userId, CancellationToken ct = default)
    {
        ExamAttempt attempt;
        Exam exam;

        if (!string.IsNullOrEmpty(request.Id))
        {
            return await GetNextQuestionsAsync(request.Id, userId, request.AnonymousToken, ct);
        }

        var now = _clock.UtcNow;
        
        exam = await _examRepository.GetByYearAsync(request.Year, ct)
            ?? throw new InvalidOperationException("Exame não encontrado no sistema.");

        if (!exam.IsSynced)
            throw new InvalidOperationException("Este simulado ainda está sendo preparado. Tente novamente em instantes.");

        if (!request.Anonymous)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            var user = await _userRepository.GetByIdAsync(userId, ct)
                ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

            if (!user.Roles.Contains(UserRole.Student.ToString(), StringComparer.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Apenas usuários do tipo aluno podem iniciar um simulado.");
        }

        attempt = ExamAttempt.Create(
            exam.Id, userId, request.Anonymous, request.AnonymousToken,
            request.TimeInMinutes, request.QuestionCount, request.Language,
            request.Humanities, request.Nature, request.Languages, request.Math, now);

        var (nextQuestions, availableCount) = await FetchNextQuestionsAsync(attempt, exam, now, ct);

        await _attemptRepository.AddAsync(attempt, ct);
        await _examRepository.IncrementStartAsync(exam.Id, now, ct);

        var responsePage = new EnemApiQuestionsPage
        {
            Metadata = new EnemApiPagination
            {
                Limit = nextQuestions.Count,
                Offset = 0,
                Total = attempt.QuestionCount,
                HasMore = attempt.QuestionCount > nextQuestions.Count
            },
            Questions = nextQuestions
        };

        return MapStartResponse(attempt, exam, responsePage);
    }

    public async Task<StartExamResponse> GetNextQuestionsAsync(string attemptId, string? userId, string? anonymousToken, CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId, ct)
            ?? throw new InvalidOperationException("Simulado não encontrado.");

        EnsureAttemptOwnership(attempt, userId, attempt.IsAnonymous, anonymousToken);

        if (attempt.FinishedAt.HasValue)
            throw new InvalidOperationException("Este simulado já foi finalizado.");

        var now = _clock.UtcNow;
        
        if (attempt.IsExpiredAt(now))
            throw new InvalidOperationException("O tempo para realizar este simulado expirou.");

        var exam = await _examRepository.GetByIdAsync(attempt.ExamId, ct)
            ?? throw new InvalidOperationException("Exame associado não encontrado.");

        var alreadyServedCount = attempt.ServedQuestions.Count;

        var (nextQuestions, availableCount) = await FetchNextQuestionsAsync(attempt, exam, now, ct);

        await _attemptRepository.UpdateAsync(attempt, ct);

        var responsePage = new EnemApiQuestionsPage
        {
            Metadata = new EnemApiPagination
            {
                Limit = nextQuestions.Count,
                Offset = alreadyServedCount,
                Total = attempt.QuestionCount,
                HasMore = attempt.ServedQuestions.Count < attempt.QuestionCount
            },
            Questions = nextQuestions
        };

        return MapStartResponse(attempt, exam, responsePage);
    }

    private async Task<(List<EnemApiQuestion> NextQuestions, int TotalAvailable)> FetchNextQuestionsAsync(
        ExamAttempt attempt, Exam exam, DateTime now, CancellationToken ct)
    {
        var examYearAsInt = int.Parse(exam.Year);

        // 1. Busca na API externa
        var page = await _enemApi.GetQuestionsAsync(
            examYearAsInt, attempt.SelectedLanguage, limit: 200, offset: 0, ct);

        // 2. Filtra disciplinas e remove duplicatas
        var selectedDisciplines = GetDisciplines(attempt);
        var availableQuestions = page.Questions
            .Where(q => selectedDisciplines.Contains(q.Discipline ?? ""))
            .DistinctBy(q => q.Index)
            .ToList();

        const int PageSize = 10;
        var remainingToServe = attempt.QuestionCount - attempt.ServedQuestions.Count;

        if (remainingToServe <= 0)
            throw new InvalidOperationException("Você já recebeu todas as questões solicitadas para este simulado.");

        var takeCount = Math.Min(PageSize, remainingToServe);

        // 3. Remove as questões já servidas e pega a quantidade correta
        var alreadyServedIndexes = attempt.ServedQuestions.Select(q => q.Index).ToHashSet();
        var nextQuestions = availableQuestions
            .Where(q => !alreadyServedIndexes.Contains(q.Index))
            .Take(takeCount)
            .ToList();

        if (nextQuestions.Count == 0)
            throw new InvalidOperationException("Não há mais questões disponíveis para esta seleção de disciplinas.");

        // 4. Salva no histórico (na memória, o Facade chamará o Repository depois)
        var newServed = nextQuestions.Select(q => new AttemptQuestion(q.Index, q.Discipline ?? "unknown", q.Language));
        attempt.AddServedQuestions(newServed, now);

        return (nextQuestions, availableQuestions.Count);
    }

    public async Task<FinishExamResponse> FinishExamAsync(FinishExamRequest request, string? userId, CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(request.Id, ct)
            ?? throw new InvalidOperationException("Simulado não encontrado ao tentar finalizar exame.");

        var exam = await _examRepository.GetByIdAsync(attempt.ExamId, ct)
            ?? throw new InvalidOperationException("Exame associado não encontrado.");

        EnsureAttemptOwnership(attempt, userId, request.Anonymous, request.AnonymousToken);

        if (attempt.FinishedAt.HasValue)
            throw new InvalidOperationException("Este simulado já foi finalizado.");

        var now = _clock.UtcNow;

        var examYearAsInt = int.Parse(exam.Year);
        var page = await _enemApi.GetQuestionsAsync(examYearAsInt, attempt.SelectedLanguage, limit: 200, offset: 0, ct);
        
        var gabarito = page.Questions
            .GroupBy(q => q.Index)
            .ToDictionary(g => g.Key, g => g.First().CorrectAlternative);

        var answers = new List<QuestionAnswer>();
    
        foreach (var q in request.Questions.DistinctBy(q => q.Index))
        {
            // Verifica se a questão realmente pertence a este simulado
            if (!attempt.ServedQuestions.Any(s => s.Index == q.Index))
                throw new InvalidOperationException($"A questão {q.Index} não pertence a este simulado.");

            // Busca o gabarito oficial na API
            if (!gabarito.TryGetValue(q.Index, out var officialCorrectAlternative))
                throw new InvalidOperationException($"Gabarito oficial da questão {q.Index} não encontrado.");

            // Cria a resposta usando a alternativa oficial + a escolha do usuário
            answers.Add(QuestionAnswer.Create(q.Index, officialCorrectAlternative, q.SelectedAlternative));
        }

        attempt.RegisterAnswers(answers, now);
        attempt.Finish(now);
        await _attemptRepository.UpdateAsync(attempt, ct);

        await _examRepository.IncrementFinishAsync(exam.Id, answers.Count, now, ct);

        return BuildReportResponse(exam, attempt);
    }

    public async Task<FinishExamResponse> GetReportAsync(string id, string? userId, CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Simulado não encontrado ao tentar obter relatório.");

        EnsureAttemptOwnership(attempt, userId, attempt.IsAnonymous, attempt.AnonymousToken);

        if (!attempt.FinishedAt.HasValue)
            throw new InvalidOperationException("Simulado ainda não foi finalizado.");

        var exam = await _examRepository.GetByIdAsync(attempt.ExamId, ct)
            ?? throw new InvalidOperationException("Exame associado não encontrado.");

        return BuildReportResponse(exam, attempt);
    }

    public async Task<ExamResultsResponse> GetResultsAsync(
        string id, 
        string? userId, 
        string? anonymousToken, 
        int limit, 
        int offset, 
        CancellationToken ct = default)
    {
        var attempt = await _attemptRepository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Simulado não encontrado.");

        EnsureAttemptOwnership(attempt, userId, attempt.IsAnonymous, anonymousToken);

        var exam = await _examRepository.GetByIdAsync(attempt.ExamId, ct)
            ?? throw new InvalidOperationException("Exame associado não encontrado.");

        // 1. Busca as questões completas na API (com contexto, imagens e alternativas)
        // Usamos limit 200 para garantir que pegamos todas as questões da prova do ENEM.
        var examYearAsInt = int.Parse(exam.Year);
        var page = await _enemApi.GetQuestionsAsync(examYearAsInt, attempt.SelectedLanguage, limit: 200, offset: 0, ct);

        // 2. Prepara os filtros e buscas O(1)
        var servedIndexes = attempt.ServedQuestions.Select(q => q.Index).ToHashSet();
        var answersByIndex = attempt.Answers.ToDictionary(a => a.Index);

        // 3. Filtra apenas as questões que foram servidas e ordena
        var allServedQuestions = page.Questions
            .Where(x => servedIndexes.Contains(x.Index))
            .OrderBy(q => q.Index)
            .ToList();

        var totalCount = allServedQuestions.Count;

        // 4. Aplica a paginação (Skip / Take)
        var paginatedQuestions = allServedQuestions
            .Skip(offset)
            .Take(limit)
            .ToList();

        var resultQuestions = new List<ExamResultQuestionDto>();

        // 5. Monta o DTO de resposta cruzando com o que o aluno respondeu
        foreach (var q in paginatedQuestions)
        {
            answersByIndex.TryGetValue(q.Index, out var answer);
            
            var selected = answer?.SelectedAlternative ?? string.Empty;
            var isCorrect = !string.IsNullOrEmpty(selected) && 
                            string.Equals(selected, q.CorrectAlternative, StringComparison.OrdinalIgnoreCase);

            resultQuestions.Add(new ExamResultQuestionDto(
                Title: q.Title,
                Index: q.Index,
                Discipline: q.Discipline,
                Language: q.Language,
                Year: q.Year,
                Context: q.Context,
                Files: q.Files,
                CorrectAlternative: q.CorrectAlternative,
                SelectedAlternative: selected,
                IsCorrect: isCorrect,
                AlternativesIntroduction: q.AlternativesIntroduction,
                Alternatives: q.Alternatives.Select(a => new AlternativeDto(a.Letter, a.Text, a.File, a.IsCorrect))
            ));
        }

        return new ExamResultsResponse(
            Id: attempt.Id,
            ExamId: exam.Id,
            Year: examYearAsInt,
            StartedAt: attempt.StartedAt,
            FinishedAt: attempt.FinishedAt,
            Metadata: new ExamMetadataDto(
                Limit: limit,
                Offset: offset,
                Total: totalCount,
                HasMore: offset + limit < totalCount
            ),
            Questions: resultQuestions
        );
    }
    
    // ===================== Helpers =====================
    private static void EnsureAttemptOwnership(ExamAttempt attempt, string? userId, bool anonymous, string? anonymousToken)
    {
        if (attempt.IsAnonymous)
        {
            if (string.IsNullOrWhiteSpace(anonymousToken) || anonymousToken != attempt.AnonymousToken)
                throw new UnauthorizedAccessException("Token anônimo inválido.");
            return;
        }

        if (string.IsNullOrEmpty(userId) || attempt.UserId != userId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este simulado.");
    }

    private static List<string> GetDisciplines(ExamAttempt attempt)
    {
        var disciplines = new List<string>();
        if (attempt.IncludeHumanities) disciplines.Add("ciencias-humanas");
        if (attempt.IncludeNature) disciplines.Add("ciencias-natureza");
        if (attempt.IncludeLanguages) disciplines.Add("linguagens");
        if (attempt.IncludeMath) disciplines.Add("matematica");
        return disciplines;
    }

    private static ExamResponseDto MapExamToDto(Exam e) => new(
        Id: e.Id,
        Title: e.Title,
        Year: e.Year,
        QuestionCountHumanities: e.QuestionCountHumanities,
        QuestionCountNature: e.QuestionCountNature,
        QuestionCountLanguages: e.QuestionCountLanguages,
        QuestionCountMath: e.QuestionCountMath,
        Languages: e.Languages);
    
    private static StartExamResponse MapStartResponse(ExamAttempt attempt, Exam exam, EnemApiQuestionsPage page)
    {
        return new StartExamResponse(
            Id: attempt.Id,
            Exam: exam.Id,
            Year: exam.Year,
            TimeInMinutes: attempt.TimeInMinutes,
            QuestionCount: attempt.QuestionCount,
            Language: attempt.SelectedLanguage,
            Humanities: attempt.IncludeHumanities,
            Nature: attempt.IncludeNature,
            Languages: attempt.IncludeLanguages,
            Math: attempt.IncludeMath,
            Anonymous: attempt.IsAnonymous,
            AnonymousToken: attempt.AnonymousToken,
            StartedAt: attempt.StartedAt,
            FinishAt: attempt.PlannedFinishAt,
            Metadata: new ExamMetadataDto(page.Metadata.Limit, page.Metadata.Offset, page.Metadata.Total, page.Metadata.HasMore),
            Questions: page.Questions.Select(MapQuestionToDto).ToList()
        );
    }

    private static QuestionDto MapQuestionToDto(EnemApiQuestion q) => new(
        Title: q.Title,
        Index: q.Index,
        Discipline: q.Discipline,
        Language: q.Language,
        Year: q.Year,
        Context: q.Context,
        Files: q.Files,
        CorrectAlternative: q.CorrectAlternative,
        AlternativesIntroduction: q.AlternativesIntroduction,
        Alternatives: q.Alternatives.Select(a => new AlternativeDto(a.Letter, a.Text, a.File, a.IsCorrect)));

    private static FinishExamResponse BuildReportResponse(Exam exam, ExamAttempt attempt)
    {
        var servedByIndex = attempt.ServedQuestions
            .GroupBy(q => q.Index)
            .ToDictionary(g => g.Key, g => g.First());

        var answersWithDiscipline = attempt.Answers
            .DistinctBy(a => a.Index)
            .Select(a =>
            {
                servedByIndex.TryGetValue(a.Index, out var served);
                return new { Answer = a, Discipline = served?.Discipline ?? "unknown" };
            }).ToList();

        var answeredPerDiscipline = answersWithDiscipline.GroupBy(x => x.Discipline).ToDictionary(g => g.Key, g => g.Count());
        var correctPerDiscipline = answersWithDiscipline.Where(x => x.Answer.IsCorrect).GroupBy(x => x.Discipline).ToDictionary(g => g.Key, g => g.Count());

        string[] friendlyKeys = ["ciencias-humanas", "ciencias-natureza", "linguagens", "matematica"];
        string[] friendlyNames = ["humanities", "nature", "languages", "math"];

        var correctAnswersDict = new Dictionary<string, int>();
        var summaryValues = new Dictionary<string, string>();

        for (var i = 0; i < friendlyKeys.Length; i++)
        {
            var apiKey = friendlyKeys[i];
            var friendly = friendlyNames[i];

            var answered = answeredPerDiscipline.GetValueOrDefault(apiKey, 0);
            var correct = correctPerDiscipline.GetValueOrDefault(apiKey, 0);

            correctAnswersDict[friendly] = correct;
            summaryValues[friendly] = answered > 0
                ? ((double)correct / answered * 100.0).ToString("F1", CultureInfo.InvariantCulture)
                : "0.0";
        }

        var totalAnswered = attempt.Answers.Count;
        var totalCorrect = attempt.Answers.Count(a => a.IsCorrect);
        var totalPercentage = totalAnswered > 0
            ? ((double)totalCorrect / totalAnswered * 100.0).ToString("F1", CultureInfo.InvariantCulture)
            : "0.0";

        var summary = new ExamSummaryDto(
            TotalPercentage: totalPercentage,
            Humanities: summaryValues["humanities"],
            Nature: summaryValues["nature"],
            Languages: summaryValues["languages"],
            Math: summaryValues["math"]);

        var report = new ExamReportDto(
            TimeInMinutes: attempt.TimeInMinutes,
            QuestionCount: attempt.QuestionCount,
            Language: attempt.SelectedLanguage,
            Humanities: attempt.IncludeHumanities,
            Nature: attempt.IncludeNature,
            Languages: attempt.IncludeLanguages,
            Math: attempt.IncludeMath,
            Anonymous: attempt.IsAnonymous,
            AnonymousToken: attempt.AnonymousToken,
            StartedAt: attempt.StartedAt,
            PlannedFinishAt: attempt.PlannedFinishAt,
            FinishAt: attempt.FinishedAt ?? DateTime.UtcNow,
            CorrectAnswers: correctAnswersDict,
            Summary: summary);

        var examInfo = new ExamInfoDto(
            Id: exam.Id,
            Year: exam.Year,
            QuestionCountHumanities: exam.QuestionCountHumanities,
            QuestionCountNature: exam.QuestionCountNature,
            QuestionCountLanguages: exam.QuestionCountLanguages,
            QuestionCountMath: exam.QuestionCountMath);

        return new FinishExamResponse(examInfo, report);
    }
}